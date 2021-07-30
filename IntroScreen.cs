namespace piskworks
{
    public class IntroScreen
    {
        private Game _game;
        private HostingButton _hostButton;
        private HostingButton _joinButton;

        public IntroScreen(Game game)
        {
            _game = game;
            _hostButton = new HostingButton(_game, 0, 0, null, HostingKind.Host);
            _joinButton = new HostingButton(_game, 0, 0, null, HostingKind.Guest);
        }

        public void DisplayIntro()
        {
            
        }
        
    }
}