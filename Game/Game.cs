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

        public Player Player { get; private set; }
        public GameBoard Board { get; private set; }
        public bool IsGameOver { get; set; }
        public bool ThisPlayerWon { get; set; }
        
        private HostingKind _hostingKind;
        private GameScreen _currentScreen;

        public Vizualizer3D Vizualizer;

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
            // TODO: Add your initialization logic here

            Window.AllowUserResizing = true;

            //_currentScreen = new IntroScreen(this);
            Board = new GameBoard(4);
            Player = new HostPlayer(this);
            _currentScreen = new PlayScreen(this, Board, true);

            Vizualizer = new Vizualizer3D(new GameBoard(4), this);
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            Texture2D fontTexture = Content.Load<Texture2D>("spritefont");
            GraphicUtils.Font = FontLoader.CreateFont(fontTexture);
            Texture2D sourceTexture = Content.Load<Texture2D>("piskworks_new");
            SpriteBank = new SpriteBank(sourceTexture);
        }

        public void RestartGame()
        {
            IsGameOver = false;
            ThisPlayerWon = false;
            Board = null;
            Player = null;
            _currentScreen = new IntroScreen(this);
        }

        public void TransitionFromIntro(HostingKind hostingKind)
        {
            _hostingKind = hostingKind;
            if (_hostingKind == HostingKind.Guest) {
                Player = new GuestPlayer(this);
                Player.Start();
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
            
            //Vizualizer.Draw(null);
            
            _currentScreen.Draw(gameTime);
            
            base.Draw(gameTime);
        }
        
    }
}
