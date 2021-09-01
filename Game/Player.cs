using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace piskworks
{

    public enum HostingKind
    {
        Host,
        Guest
    }
    
    public abstract class Player
    {
        protected const int PORT = 60_525;

        public bool WaitingForResponse;

        public SymbolKind PlayerSymbol;
        protected Game _game;

        public IComunicator Comunicator;
        
        public Player(Game game)
        {
            _game = game;
        }
        
        public abstract void DealWithMsg();
        public abstract void DoMove(int x, int y, int z);
        public abstract void Start();
        public abstract void Stop();
        
        public void AnnounceWinner(bool thisPlayerWon)
        {
            _game.IsGameOver = true;
            _game.ThisPlayerWon = thisPlayerWon;
        }

        public void AnnounceQuitingGame()
        {
            Comunicator.Send(MessageObject.CreateGameOverMsg(false));
        }

    }

    public class HostPlayer : Player
    {
        private TcpListener _listener;
        private bool _listening;
        public HostPlayer(Game game) : base(game)
        {
            PlayerSymbol = SymbolKind.Cross;
            //Comunicator = new TestComunicator();
            _listening = false;
        }
        
        public override async void Start()
        {
            var cancelationTokenSource = new CancellationTokenSource();
            var cancelationToken = cancelationTokenSource.Token;
            _game.SetCurScreen(new MessageScreen(_game, null, () => cancelationTokenSource.Cancel()));
            var connectTask = Task.Run(() => connectOtherPlayer(cancelationToken), cancelationToken);
            var connected = false;
            try {
                await connectTask;
                connected = true;
            }
            catch (OperationCanceledException e) {
                _listener.Stop();
            }
            if (!connected) return;
            shareDimension();
            _game.SetCurScreen(new PlayScreen(_game, _game.Board, true));
            WaitingForResponse = false;
        }

        public override void Stop()
        {
            if (_listening) {
                //_listener.Stop();
            }
            Comunicator?.EndComunication();
        }

        private async Task connectOtherPlayer(CancellationToken token)
        {
            _listener = new TcpListener(IPAddress.Any, PORT);
            _listener.Start();
            _listening = true;
            Console.WriteLine($"listening on port {PORT}");
            await using (token.Register(() => _listener.Stop())) {
                try {
                    var client = await _listener.AcceptTcpClientAsync();
                    Console.WriteLine("connected");
                    _listener.Stop();
                    _listening = false;
                    Comunicator = new ComunicatorTcp(client);
                    Comunicator.StartComunication();
                }
                catch (InvalidOperationException)
                {
                    token.ThrowIfCancellationRequested();
                    throw;
                }   
            }

        }

        private void shareDimension()
        {
            Comunicator.Send(MessageObject.CreateDimensionMsg(_game.Board.N));
        }
        
        public override void DealWithMsg()
        {
            var msg = Comunicator.Receive();
            if (msg.Kind == MessageKind.Move) {
                dealWithMove(msg.Move);
            }
            else if (msg.Kind == MessageKind.GameOver && !msg.IsWinning) {
                // game was quit by the other user
                _game.QuitGame(false);
            }
        }

        private void dealWithMove(GameMove move)
        {
            if (WaitingForResponse) {
                //Console.WriteLine(move.ToString());
                _game.Board.DoMove(move);
                if (_game.Board.CheckForWin(move.Symbol)) {
                    // other player had the winning move
                    Comunicator.Send(MessageObject.CreateGameOverMsg(true));
                    AnnounceWinner(thisPlayerWon: false);
                }
                WaitingForResponse = false;
            }
            else {
                throw new ArgumentException("Unexpected move received");
            }
        }

        public override void DoMove(int x, int y, int z)
        {
            var move = new GameMove(x, y, z, PlayerSymbol);
            _game.Board.DoMove(move);
            if (_game.Board.CheckForWin(PlayerSymbol)) {
                // this move was the winning move
                Comunicator.Send(MessageObject.CreateMoveMsg(move, true));
                AnnounceWinner(thisPlayerWon: true);
            }
            else {
                Comunicator.Send(MessageObject.CreateMoveMsg(move));
                WaitingForResponse = true;   
            }
        }
    }

    public class GuestPlayer : Player
    {

        public GuestPlayer(Game game) : base(game)
        {
            PlayerSymbol = SymbolKind.Nought;
        }

        public override async void Start()
        {
            _game.SetCurScreen(new MessageScreen(_game));
            await connectOtherPlayer();
            var dimension = await getDimension();
            _game.CreateBoard(dimension);
            _game.SetCurScreen(new PlayScreen(_game, _game.Board, false));
            WaitingForResponse = true;
        }

        public override void Stop()
        {
            Comunicator?.EndComunication();
        }

        public override void DealWithMsg()
        {
            var msg = Comunicator.Receive();
            if (msg.Kind == MessageKind.Move) {
                _game.Board.DoMove(msg.Move);
                if (msg.IsWinning) {
                    var oponentSymbol = PlayerSymbol == SymbolKind.Cross ? SymbolKind.Nought : SymbolKind.Cross;
                    _game.Board.CheckForWin(oponentSymbol);
                    AnnounceWinner(false);
                }
                WaitingForResponse = false;
            }
            else if (msg.Kind == MessageKind.GameOver) {
                if (msg.IsWinning) {
                    _game.Board.CheckForWin(PlayerSymbol);
                    AnnounceWinner(true);
                }
                else {
                    // game was quit by the other user
                    _game.QuitGame(false);
                }
                
            }
        }

        public override void DoMove(int x, int y, int z)
        {
            var move = new GameMove(x, y, z, PlayerSymbol);
            _game.Board.DoMove(move);
            Comunicator.Send(MessageObject.CreateMoveMsg(move));
            WaitingForResponse = true;  
        }

        private async Task<int> getDimension()
        {
            await Task.Factory.StartNew(() => {
                while (!Comunicator.IsMsgAvailable()) {
                    Thread.Sleep(100);
                }
            });
            var msg = Comunicator.Receive();
            if (msg.Kind == MessageKind.Dimension) {
                return msg.Dimension;
            }
            else {
                throw new ArgumentException("First message should share dimension, but was type " + msg.GetType());
            }
        }

        private async Task connectOtherPlayer()
        {
            var client = new TcpClient();
            IPAddress ipAddress = null;
            var hasAddress = false;
            while (!hasAddress) {
                Console.WriteLine("Write IP address of host player: (IPv6)" );
                var addressStr = Console.ReadLine();
                hasAddress = IPAddress.TryParse(addressStr, out ipAddress);
                if (hasAddress && ipAddress.AddressFamily is AddressFamily.InterNetwork ) {
                    Console.WriteLine("You entered and address in IPv4, you need to convert it to IPv6");
                    hasAddress = false;
                }
            }
            //ipAddress = Dns.GetHostAddresses("localhost")[0];
            var notConnected = true;
            while (notConnected) {
                try {
                    await client.ConnectAsync(ipAddress, PORT);
                    notConnected = false;
                }
                catch (SocketException e) {
                }
            }
            
            Comunicator = new ComunicatorTcp(client);
            Comunicator.StartComunication();
        }
    }
}