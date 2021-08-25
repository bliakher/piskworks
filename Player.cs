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
        public bool WaitingForResponse;

        public SymbolKind PlayerSymbol;
        protected Game _game;

        private IPAddress _serverAddress;
        private int _serverPort;

        private TcpClient _client;
        private NetworkStream _stream;
        private StreamWriter _writer;
        private StreamReader _reader;

        public IComunicator Comunicator;
        
        public Player(Game game)
        {
            _game = game;
            PlayerSymbol = SymbolKind.Cross;
            //WaitingForResponse = false;
            _serverAddress = Dns.GetHostEntry("localhost").AddressList[0];
            _serverPort = 50000;
        }
        
        public abstract void DealWithMsg();
        public abstract void DoMove(int x, int y, int z);
        public abstract void ConnectOtherPlayer();
        public abstract void Start();
        
        public void InitializeComunicator(ThreadSafeQueue<MessageObject> sendQueue,
            ThreadSafeQueue<MessageObject> receiveQueue)
        {
            Comunicator = new ComunicatorMock(sendQueue, receiveQueue);
        }

        public async Task ConnectToServer()
        {
            _client = new TcpClient();
            await _client.ConnectAsync(_serverAddress, _serverPort);
            Console.WriteLine("Connecting to server.");
            _stream = _client.GetStream();
            //_stream.ReadTimeout = 2000;
            _reader = new StreamReader(_stream);
            _writer = new StreamWriter(_stream);
            Console.WriteLine("Connected.");
        }

        // public async void SendMessageToServer(MessageObject msg)
        // {
        //     var msgString = JsonSerializer.Serialize<MessageObject>(msg);
        //     await _writer.WriteLineAsync(msgString);
        //     await _writer.FlushAsync();
        //     
        //     WaitingForResponse = true;
        // }

        public async void CheckPendingMsg()
        {
            if (_client.Available > 0) {
                var msgString = await _reader.ReadLineAsync();
                Console.WriteLine("Got a response: " + msgString);
                WaitingForResponse = false;
                MessageObject msg = JsonSerializer.Deserialize<MoveMsgObject>(msgString);
                //DealWithMsg(msg);
            }
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
            Comunicator = new TestComunicator();
        }
        
        public override void Start()
        {
            _game.SetCurScreen(new WaitScreen(_game));
            //ConnectOtherPlayer(); // ToDo: react to possible exceptions
            _game.SetCurScreen(new PlayScreen(_game, _game.Board, true));
            WaitingForResponse = false;
        }

        public override async void ConnectOtherPlayer()
        {
            await Task.Factory.StartNew(() => Thread.Sleep(2000));
        }
        
        public override void DealWithMsg()
        {
            var msg = Comunicator.Receive();
            if (msg is MoveMsgObject moveObj) {
                dealWithMove(moveObj.Move);
            }
        }

        private void dealWithMove(GameMove move)
        {
            if (WaitingForResponse) {
                _game.Board.DoMove(move);
                if (_game.Board.CheckForWin(move.Symbol)) {
                    // other player had the winning move
                    Comunicator.Send(new MessageObject(){Kind = MessageKind.GameEnd});
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
                Comunicator.Send(new MoveMsgObject(move, true));
                AnnounceWinner(thisPlayerWon: true);
            }
            else {
                Comunicator.Send(new MoveMsgObject(move));
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

        public override void Start()
        {
            _game.SetCurScreen(new WaitScreen(_game));
            ConnectOtherPlayer(); // ToDo: react to possible exceptions
            _game.SetCurScreen(new PlayScreen(_game, _game.Board, false));
            WaitingForResponse = true;
        }
        public override void DealWithMsg()
        {
            var msg = Comunicator.Receive();
            if (msg is MoveMsgObject moveObj) {
                _game.Board.DoMove(moveObj.Move);
                WaitingForResponse = false;
            }
        }

        public override void DoMove(int x, int y, int z)
        {
            var move = new GameMove(x, y, z, PlayerSymbol);
            _game.Board.DoMove(move);
            Comunicator.Send(new MoveMsgObject(move));
            WaitingForResponse = true;  
        }

        public override void ConnectOtherPlayer()
        {
            
        }
    }

    public class Server
    {
        public HostPlayer host;
        public GuestPlayer guest;
        private GameBoard _board;

        const int _port = 50000;
        private TcpListener _listener;

        private TcpClient _player1;
        private TcpClient _player2;

        private IComunicator _hostComunicator;
        private IComunicator _joinComunicator;

        private List<MoveMsgObject> _testMoves;

        public Server(int boardDim)
        {
            _board = new GameBoard(boardDim);
            _testMoves = new List<MoveMsgObject>();
            getTestMsgs();
        }

        public void InitializeHostComunicator(ThreadSafeQueue<MessageObject> sendQueue,
            ThreadSafeQueue<MessageObject> receiveQueue)
        {
            _hostComunicator = new ComunicatorMock(sendQueue, receiveQueue);
        }

        public async void Run()
        {
            // var ipAddress = Dns.GetHostEntry("localhost").AddressList[0];
            // _listener = new TcpListener(ipAddress, _port);
            // _listener.Start();
            //
            // _player1 = await _listener.AcceptTcpClientAsync();
            // Console.WriteLine("Server connected");
            //
            // var stream = _player1.GetStream();
            // var writer = new StreamWriter(stream);
            // var reader = new StreamReader(stream);
            
            // wait for join player
            Thread.Sleep(3000); // simulating waiting
            // when he connects, send initial msg to players
            _hostComunicator.Send(new MessageObject(){Kind = MessageKind.Initial});
            Console.WriteLine("initial msg sent");
            //_joinComunicator.Send(new MessageObject(){Kind = MessageKind.Initial});
            
            // alternate receiving moves from players
            var hostWon = false;
            var i = 0;
            while (true) {
                var moveHost = waitForValidMove(_hostComunicator);
                
                // received valid move - is already played on board
                // check if player won
                if (_board.CheckForWin(moveHost.Symbol)) {
                    hostWon = true;
                    _hostComunicator.Send(new MessageObject(){Kind = MessageKind.GameEnd});
                    //_joinComunicator.Send(new MoveMsgObject(moveHost, true));
                    break;
                }
                //_joinComunicator.Send(new MoveMsgObject(moveHost));
                
                // var moveJoin = waitForValidMove(_joinComunicator);
                // for testing
                var moveJoin = _testMoves[i].Move;
                i++;
                
                if (_board.CheckForWin(moveJoin.Symbol)) {
                    hostWon = false;
                    //_joinComunicator.Send(new MessageObject(){Kind = MessageKind.GameEnd});
                    _hostComunicator.Send(new MoveMsgObject(moveHost, true));
                    break;
                }
                _hostComunicator.Send(new MoveMsgObject(moveJoin));

            }


            // var i = 0;
            // while (i < 4) {
            //     
            //
            //     Console.WriteLine("Sending move");
            //     var msgOut = _testMoves[i];
            //     var msgOutStr = JsonSerializer.Serialize(msgOut);
            //
            //     await writer.WriteLineAsync(msgOutStr);
            //     await writer.FlushAsync();
            //     Console.WriteLine("Move sent.");
            //     Console.WriteLine("Move:" + msgOutStr);
            //
            //
            //     while (_player1.Available <= 0) {
            //         continue;
            //     }
            //     var msgStr = await reader.ReadLineAsync();
            //     var msg = JsonSerializer.Deserialize<MessageObject>(msgStr);
            //     Console.WriteLine("Server received msg");
            //     
            //     i++;
            // }
        }

        private MessageObject waitForMsg(IComunicator playerCom)
        {
            // look if msg available, if not sleep and try again
            while (!playerCom.IsMsgAvailable()) {
                Thread.Sleep(100);
            }
            return playerCom.Receive();
        }

        private GameMove waitForValidMove(IComunicator playerCom)
        {
            while (true) {
                var msg = waitForMsg(playerCom);
                if (msg is MoveMsgObject moveMsg) {
                    var move = moveMsg.Move;
                    var valid = _board.DoMove(move);
                    if (valid) {
                        return move;
                    }
                    Console.WriteLine("Server received invalid move");
                }
            }
        }

        private void getTestMsgs()
        {
            _testMoves.Add(new MoveMsgObject(new GameMove(1, 0, 0, SymbolKind.Nought)));
            _testMoves.Add(new MoveMsgObject(new GameMove(1, 1, 0, SymbolKind.Nought)));
            _testMoves.Add(new MoveMsgObject(new GameMove(1, 2, 0, SymbolKind.Nought)));
            _testMoves.Add(new MoveMsgObject(new GameMove(1, 3, 0, SymbolKind.Nought)));
        }
    }

    public interface IComunicator
    {
        public void Send(MessageObject msg);
        public MessageObject Receive();

        public bool IsMsgAvailable();
    }

    public class ComunicatorMock : IComunicator
    {
        private ThreadSafeQueue<MessageObject> _sendQueue;
        private ThreadSafeQueue<MessageObject> _receiveQueue;

        public ComunicatorMock(ThreadSafeQueue<MessageObject> sendQueue, ThreadSafeQueue<MessageObject> receiveQueue)
        {
            _sendQueue = sendQueue;
            _receiveQueue = receiveQueue;
        }

        public void Send(MessageObject msg)
        {
            _sendQueue.Enqueue(msg);
        }

        public MessageObject Receive()
        {
            return _receiveQueue.Dequeue();
        }

        public bool IsMsgAvailable()
        {
            return _receiveQueue.Count > 0;
        }
    }

    public class TestComunicator : IComunicator
    {
        private int i = 0;
        public List<MoveMsgObject> _testMoves = new List<MoveMsgObject> {
            new MoveMsgObject(new GameMove(1, 0, 0, SymbolKind.Nought)),
            new MoveMsgObject(new GameMove(1, 1, 0, SymbolKind.Nought)),
            new MoveMsgObject(new GameMove(1, 2, 0, SymbolKind.Nought)),
            new MoveMsgObject(new GameMove(1, 3, 0, SymbolKind.Nought))
        };
        public void Send(MessageObject msg)
        {
        }

        public MessageObject Receive()
        {
            var move = _testMoves[i];
            i++;
            return move;
        }

        public bool IsMsgAvailable()
        {
            return i < 4;
        }
    }
}