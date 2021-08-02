using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace piskworks
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;
        public SpriteBatch SpriteBatch;

        private HostingKind _hostingKind;
        public Player Player;
        public Viewer Viewer;

        private GameScreen _currentScreen;
        

        public Game()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Viewer = new Viewer(new GameBoard(4), this);
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            _currentScreen = new IntroScreen(this);
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            Texture2D fontTexture = Content.Load<Texture2D>("spritefont");
            GraphicUtils.Font = FontLoader.CreateFont(fontTexture);

            // TODO: use this.Content to load your game content here
        }

        private void startGame()
        {
            if (_hostingKind == HostingKind.Guest) {
                startGameGuest();
            }
            else {
                startGameHost();
            }
        }

        private void startGameHost()
        {
            var server = new Server();
            Player = new HostPlayer(server);
        }

        private void startGameGuest()
        {
            Player = new GuestPlayer();
            _currentScreen = new WaitScreen(this);
        }

        public void TransitionFromIntro(HostingKind hostingKind)
        {
            _hostingKind = hostingKind;
            startGame();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            
            _currentScreen.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            //Viewer.Draw3DVizualization(gameTime);
            
            _currentScreen.Draw(gameTime);
            
            base.Draw(gameTime);
        }
        
    }
}
