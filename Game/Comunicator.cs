using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;

namespace piskworks
{
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
            var msgText = JsonSerializer.Serialize<MessageObject>(msg);
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
        public List<MessageObject> _testMoves = new List<MessageObject>();
        // ToDo: repair test move objects
        //     {new MoveMsgObject(new GameMove(1, 0, 0, SymbolKind.Nought)),
        //     new MoveMsgObject(new GameMove(1, 1, 0, SymbolKind.Nought)),
        //     new MoveMsgObject(new GameMove(1, 2, 0, SymbolKind.Nought)),
        //     new MoveMsgObject(new GameMove(1, 3, 0, SymbolKind.Nought))
        // };
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