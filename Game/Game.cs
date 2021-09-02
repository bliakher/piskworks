using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace piskworks
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;
        public SpriteBatch SpriteBatch;
        public SpriteBank SpriteBank;
        public Model3D Nougth;
        public Model3D Cross;

        public Player Player { get; private set; }
        public GameBoard Board { get; private set; }
        public bool IsGameOver { get; set; }
        public bool ThisPlayerWon { get; set; }
        
        public string PlayerEnteredText { get; set; }
        
        private HostingKind _hostingKind;
        private GameScreen _currentScreen;

        public Game()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsGameOver = false;
            ThisPlayerWon = false;
        }

        protected override void Initialize()
        {
            Window.AllowUserResizing = true;

            _currentScreen = new IntroScreen(this);
            
            //for testing
            // Board = new GameBoard(5);
            // Board.FillForTesting();
            // Player = new HostPlayer(this);
            // _currentScreen = new PlayScreen(this, Board, true);
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            Texture2D fontTexture = Content.Load<Texture2D>("spritefont");
            GraphicUtils.Font = FontLoader.CreateFont(fontTexture);
            Texture2D sourceTexture = Content.Load<Texture2D>("piskworks_new");
            SpriteBank = new SpriteBank(sourceTexture);
            var loader = new Model3DLoader(GraphicsDevice);
            Nougth = loader.LoadWithColor("Content/kolecko.obj", new Color(167, 29, 38)); // PiskRed
            Cross = loader.LoadWithColor("Content/krizek.obj", new Color(0, 87, 132)); // PiskBlue
        }

        public void RestartGame()
        {
            IsGameOver = false;
            ThisPlayerWon = false;
            Board = null;
            Player.Stop();
            Player = null;
            _currentScreen = new IntroScreen(this);
        }

        public void QuitGame(bool youQuit)
        {
            if (youQuit) {
                SetCurScreen(new MessageScreen(this, "You quit the game."));
            }
            else {
                SetCurScreen(new MessageScreen(this, "The other user quit the game."));
            }
        }
        public void TransitionFromIntro(HostingKind hostingKind)
        {
            _hostingKind = hostingKind;
            if (_hostingKind == HostingKind.Guest) {
                _currentScreen = new TextInputScreen(this);
            }
            else {
                _currentScreen = new DimensionScreen(this);
            }
        }
        public void TransitionFromDimension(int dimension)
        {
            CreateBoard(dimension);
            Player = new HostPlayer(this);
            Player.Start();
        }

        public void TransitionFromTextInput(string text)
        {
            // check ip adress
            // try again or start the player
            var hasAddress = IPAddress.TryParse(text, out var ipAddress);
            if (hasAddress && !(ipAddress.AddressFamily is AddressFamily.InterNetwork) ) {
                Player = new GuestPlayer(this, ipAddress); // give him the ip address
                Player.Start();
            }
            else {
                _currentScreen = new TextInputScreen(this, true);
            }
        }

        public void SetCurScreen(GameScreen newScreen)
        {
            _currentScreen = newScreen;
        }

        public void CreateBoard(int dimension)
        {
            Board = new GameBoard(dimension);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            
            _currentScreen.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            _currentScreen.Draw(gameTime);
            
            base.Draw(gameTime);
        }
        
    }
}
