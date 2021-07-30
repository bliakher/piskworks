namespace piskworks
{
    public struct MessageObject
    {
        
    }

    public enum HostingKind
    {
        Host,
        Guest
    }
    
    public abstract class Player
    {
        public GameBoard _board;
        public SymbolKind _playerSymbol;

        public abstract void SendMeMessage(MessageObject msg);
        public abstract void SendMessageToServer(MessageObject msg);

        public void ReceiveMessage(MessageObject msg)
        {
            
        }
    }

    public class HostPlayer : Player
    {
        private Server _server;

        public HostPlayer(Server server)
        {
            _server = server;
            _playerSymbol = SymbolKind.Cross;
        }

        public override void SendMeMessage(MessageObject msg)
        {
            ReceiveMessage(msg);
        }

        public override void SendMessageToServer(MessageObject msg)
        {
            throw new System.NotImplementedException();
        }
    }

    public class GuestPlayer : Player
    {
        public GuestPlayer()
        {
            _playerSymbol = SymbolKind.Nought;
        }

        public override void SendMeMessage(MessageObject msg)
        {
            throw new System.NotImplementedException();
        }

        public override void SendMessageToServer(MessageObject msg)
        {
            throw new System.NotImplementedException();
        }
    }

    public class Server
    {
        private HostPlayer host;
        private GuestPlayer guest;

        public Server()
        {
        }

        private void FindSecondPlayer()
        {
            
        }

        public void RunGame()
        {
            
        }
    }
}