using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using piskworks.Graphics;

namespace piskworks.GameSrc
{

    public enum HostingKind
    {
        Host,
        Guest
    }
    
    /// <summary>
    /// Player representing the user playing the game
    /// </summary>
    public abstract class Player
    {
        protected const int PORT = 60_525;

        /// <summary>
        /// True if waiting for response from the oponent.
        /// </summary>
        public bool WaitingForResponse;

        /// <summary>
        /// Indicator if playing as nought or as cross.
        /// </summary>
        public SymbolKind PlayerSymbol;
        protected Game _game;

        /// <summary>
        /// Comunicator that handles comunication with the other player.
        /// </summary>
        public ICommunicator Communicator;
        
        public Player(Game game)
        {
            _game = game;
        }
        
        /// <summary>
        /// Receive the message and deal with it based on the content.
        /// </summary>
        public abstract void DealWithMsg();
        /// <summary>
        /// Place the <see cref="PlayerSymbol"/> on the board on specified coordinates.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="z">Z coordinate</param>
        public abstract void DoMove(int x, int y, int z);
        /// <summary>
        /// Start running.
        /// Connect with the oponent and start comunication.
        /// </summary>
        public abstract void Start();
        /// <summary>
        /// Stop the comunication with oponent.
        /// </summary>
        public abstract void Stop();
        
        /// <summary>
        /// Singnal that the game is finished with the specified result.
        /// </summary>
        /// <param name="thisPlayerWon">True if this player won, false if the oponent won</param>
        public void AnnounceWinner(bool thisPlayerWon)
        {
            _game.IsGameOver = true;
            _game.ThisPlayerWon = thisPlayerWon;
        }

        /// <summary>
        /// Signal the interruption of a running game to the oponent.
        /// </summary>
        public void AnnounceQuitingGame()
        {
            Communicator.Send(MessageObject.CreateGameOverMsg(false));
        }

    }

    /// <summary>
    /// Player with the role of a server.
    /// The other player connects to this player.
    /// </summary>
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
            Communicator?.EndComunication();
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
                    Communicator = new CommunicatorTcp(client);
                    Communicator.StartComunication();
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
            Communicator.Send(MessageObject.CreateDimensionMsg(_game.Board.N));
        }
        
        public override void DealWithMsg()
        {
            var msg = Communicator.Receive();
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
                    Communicator.Send(MessageObject.CreateGameOverMsg(true));
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
                Communicator.Send(MessageObject.CreateMoveMsg(move, true));
                AnnounceWinner(thisPlayerWon: true);
            }
            else {
                Communicator.Send(MessageObject.CreateMoveMsg(move));
                WaitingForResponse = true;   
            }
        }
    }

    /// <summary>
    /// Player with the role of a client.
    /// This player connects to the oponent that acts as a server
    /// </summary>
    public class GuestPlayer : Player
    {
        private IPAddress _hostAddress;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Instance of the game</param>
        /// <param name="hostAddress">Address of the host player</param>
        public GuestPlayer(Game game, IPAddress hostAddress) : base(game)
        {
            PlayerSymbol = SymbolKind.Nought;
            _hostAddress = hostAddress;
        }

        public override async void Start()
        {
            _game.SetCurScreen(new MessageScreen(_game));
            await connectOtherPlayer(_hostAddress);
            var dimension = await getDimension();
            _game.CreateBoard(dimension);
            _game.SetCurScreen(new PlayScreen(_game, _game.Board, false));
            WaitingForResponse = true;
        }

        public override void Stop()
        {
            Communicator?.EndComunication();
        }

        public override void DealWithMsg()
        {
            var msg = Communicator.Receive();
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
            Communicator.Send(MessageObject.CreateMoveMsg(move));
            WaitingForResponse = true;  
        }

        private async Task<int> getDimension()
        {
            await Task.Factory.StartNew(() => {
                while (!Communicator.IsMsgAvailable()) {
                    Thread.Sleep(100);
                }
            });
            var msg = Communicator.Receive();
            if (msg.Kind == MessageKind.Dimension) {
                return msg.Dimension;
            }
            else {
                throw new ArgumentException("First message should share dimension, but was type " + msg.GetType());
            }
        }

        private async Task connectOtherPlayer(IPAddress ipAddress)
        {
            var client = new TcpClient();
            var notConnected = true;
            while (notConnected) {
                try {
                    await client.ConnectAsync(ipAddress, PORT);
                    notConnected = false;
                }
                catch (SocketException e) {
                }
            }
            Communicator = new CommunicatorTcp(client);
            Communicator.StartComunication();
        }
    }
}