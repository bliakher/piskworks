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
        
        public void InitializeComunicator(ThreadSafeQueue<MessageObject> sendQueue,
            ThreadSafeQueue<MessageObject> receiveQueue)
        {
            Comunicator = new ComunicatorMock(sendQueue, receiveQueue);
        }
        
        public void AnnounceWinner(bool thisPlayerWon)
        {
            _game.IsGameOver = true;
            _game.ThisPlayerWon = thisPlayerWon;
        }

    }

    public class HostPlayer : Player
    {
        public HostPlayer(Game game) : base(game)
        {
            PlayerSymbol = SymbolKind.Cross;
            //Comunicator = new TestComunicator();
        }
        
        public override async void Start()
        {
            _game.SetCurScreen(new WaitScreen(_game));
            await connectOtherPlayer(); // ToDo: react to possible exceptions
            shareDimension();
            _game.SetCurScreen(new PlayScreen(_game, _game.Board, true));
            WaitingForResponse = false;
        }

        private async Task connectOtherPlayer()
        {
            var ipAddress = Dns.GetHostEntry("localhost").AddressList[0];
            var listener = new TcpListener(ipAddress, PORT);
            listener.Start();
            Console.WriteLine($"listening on port {PORT}");
            var client = await listener.AcceptTcpClientAsync();
            Console.WriteLine("connected");
            listener.Stop();
            Comunicator = new ComunicatorTcp(client);
            Comunicator.StartComunication();
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
        }

        private void dealWithMove(GameMove move)
        {
            if (WaitingForResponse) {
                //Console.WriteLine(move.ToString());
                _game.Board.DoMove(move);
                if (_game.Board.CheckForWin(move.Symbol)) {
                    // other player had the winning move
                    Comunicator.Send(MessageObject.CreateGameOverMsg());
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
            _game.SetCurScreen(new WaitScreen(_game));
            await connectOtherPlayer(); // ToDo: react to possible exceptions
            var dimension = await getDimension();
            _game.CreateBoard(dimension);
            _game.SetCurScreen(new PlayScreen(_game, _game.Board, false));
            WaitingForResponse = true;
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
                _game.Board.CheckForWin(PlayerSymbol);
                AnnounceWinner(true);
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
            }
            //ipAddress = Dns.GetHostAddresses("localhost")[0];
            await client.ConnectAsync(ipAddress, PORT);
            Comunicator = new ComunicatorTcp(client);
            Comunicator.StartComunication();
        }
    }
    
}