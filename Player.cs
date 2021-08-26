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

        public void AnnounceWinner(bool thisPlayerWon)
        {
            _game.IsGameOver = true;
            _game.ThisPlayerWon = thisPlayerWon;
        }

    }

    public class HostPlayer : Player
    {
        private const int _port = 50_000;
        
        public HostPlayer(Game game) : base(game)
        {
            PlayerSymbol = SymbolKind.Cross;
            //Comunicator = new TestComunicator();
        }
        
        public override async void Start()
        {
            _game.SetCurScreen(new WaitScreen(_game));
            await connectOtherPlayer(); // ToDo: react to possible exceptions
            _game.SetCurScreen(new PlayScreen(_game, _game.Board, true));
            WaitingForResponse = false;
        }

        private async Task connectOtherPlayer()
        {
            var ipAddress = Dns.GetHostEntry("localhost").AddressList[0];
            var listener = new TcpListener(ipAddress, _port);
            listener.Start();
            var client = await listener.AcceptTcpClientAsync();
            Comunicator = new ComunicatorTcp(client);
            Comunicator.StartComunication();
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

        public override  async void Start()
        {
            _game.SetCurScreen(new WaitScreen(_game));
            await connectOtherPlayer(); // ToDo: react to possible exceptions
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

        private async Task connectOtherPlayer()
        {
            
        }
    }

    public interface IComunicator
    {
        public void Send(MessageObject msg);
        public MessageObject Receive();
        public bool IsMsgAvailable();
        public void StartComunication();
        public void EndComunication();

    }

    public class ComunicatorTcp : IComunicator
    {
        private bool _comunicationEnded;
        
        private ThreadSafeQueue<MessageObject> _sendQueue;
        private ThreadSafeQueue<MessageObject> _receiveQueue;
        
        private TcpClient _tcpClient;
        private StreamReader _reader;
        private StreamWriter _writer;

        public ComunicatorTcp(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            _sendQueue = new ThreadSafeQueue<MessageObject>();
            _receiveQueue = new ThreadSafeQueue<MessageObject>();
            _comunicationEnded = false;
        }

        private void runNetworkComunication()
        {
            while (!_comunicationEnded) {
                var didSth = false;
                // send messages if available
                if (_sendQueue.Count > 0) {
                    doSend();
                    didSth = true;
                }
                // receive messages if available
                if (_tcpClient.Available > 0) {
                    doReceive();
                    didSth = true;
                }
                // if nothing to do - wait
                if (!didSth) {
                    Thread.Sleep(100);
                }
            }
            
        }
        private void initializeStreams()
        {
            var stream = _tcpClient.GetStream();
            _reader = new StreamReader(stream);
            _writer = new StreamWriter(stream);
        }

        private async void doSend()
        {
            var msg = _sendQueue.Dequeue();
            var msgText = JsonSerializer.Serialize(msg);
            await _writer.WriteLineAsync(msgText);
            await _writer.FlushAsync();
        }

        private async void doReceive()
        {
            var msgText = await _reader.ReadLineAsync();
            var msg = JsonSerializer.Deserialize<MessageObject>(msgText);
            _receiveQueue.Enqueue(msg);
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

        public void StartComunication()
        {
            initializeStreams();
            var t = new Thread(runNetworkComunication);
            t.Start();
        }

        public void EndComunication()
        {
            _comunicationEnded = true;
        }
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

        public void StartComunication()
        {
            // empty operation in mock
        }

        public void EndComunication()
        {
            // empty operation in mock
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

        public void StartComunication()
        {
            // empty operation in testing
        }

        public void EndComunication()
        {
            // empty operation in testing
        }
    }
}