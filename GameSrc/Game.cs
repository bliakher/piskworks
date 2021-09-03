using System.Net;
using System.Net.Sockets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using piskworks.Graphics;
using piskworks.Utils;

namespace piskworks.GameSrc
{
    /// <summary>
    /// The main entrypoint to the game.
    ///
    /// Initializes game components and loads content.
    /// Contains methods for controlling the game
    /// - setting current screen displayed to player
    /// - ending and restarting game
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;
        public SpriteBatch SpriteBatch;
        public SpriteBank SpriteBank;
        
        /// <summary>
        /// 3D model of the nought symbol
        /// </summary>
        public Model3D Nougth;
        /// <summary>
        /// 3D model of the cross symbol
        /// </summary>
        public Model3D Cross;

        /// <summary>
        /// Player on one side of the game - represents the user
        /// </summary>
        public Player Player { get; private set; }
        /// <summary>
        /// Cube game board
        /// </summary>
        public GameBoard Board { get; private set; }
        
        /// <summary>
        /// Indicator if game is over
        /// </summary>
        public bool IsGameOver { get; set; }
        
        /// <summary>
        /// True if this player won, false if the oponent won
        /// </summary>
        public bool ThisPlayerWon { get; set; }
        
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

        /// <summary>
        /// Restart the game.
        ///
        /// Sets values to initial values. Disconnects the player and displays the initial screen.
        /// </summary>
        public void RestartGame()
        {
            IsGameOver = false;
            ThisPlayerWon = false;
            Board = null;
            Player.Stop();
            Player = null;
            _currentScreen = new IntroScreen(this);
        }

        /// <summary>
        /// Stops a running game. Displayes a message depending on which player quit the game.
        /// </summary>
        /// <param name="youQuit">Defines if the player or the oponent quit the game</param>
        public void QuitGame(bool youQuit)
        {
            if (youQuit) {
                SetCurScreen(new MessageScreen(this, "You quit the game."));
            }
            else {
                SetCurScreen(new MessageScreen(this, "The other user quit the game."));
            }
        }
        
        /// <summary>
        /// Does a transition from the intro screen to the next screen, depending on the role of the player.
        /// Should be called only from <see cref="IntroScreen"/>.
        /// </summary>
        /// <param name="hostingKind">Role of player in connecting - host or guest</param>
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
        
        /// <summary>
        /// Does a transition from the dimension screen.
        /// 
        /// Dimension screen is only shown to the host player,
        /// so it initializes Player as <see cref="HostPlayer"/> and starts the player.
        /// Should be called only from <see cref="DimensionScreen"/>
        /// </summary>
        /// <param name="dimension">Dimension of the cube-board</param>
        public void TransitionFromDimension(int dimension)
        {
            CreateBoard(dimension);
            Player = new HostPlayer(this);
            Player.Start();
        }

        /// <summary>
        /// Does a transition from the text input screen.
        /// 
        /// It is only shown to the guest player,
        /// it checks if the text input is a valid IP address.
        /// Then it initializes Player as <see cref="GuestPlayer"/> and starts the player.
        /// If the address is not valid, displays the text input again.
        /// Should be called only from <see cref="TextInput"/>
        /// </summary>
        /// <param name="text">Text filled in the text input</param>
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

        /// <summary>
        /// Sets the current screen displayed to the user.
        /// </summary>
        /// <param name="newScreen">New <see cref="GameScreen"/></param>
        public void SetCurScreen(GameScreen newScreen)
        {
            _currentScreen = newScreen;
        }

        /// <summary>
        /// Initializes the board with specified dimension
        /// </summary>
        /// <param name="dimension">Dimension of the board</param>
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
