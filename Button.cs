using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace piskworks
{
    public class Button : DrawableGameComponent
    {
        public int X { get;  }
        public int Y { get; }
        public Texture2D Texture { get; }
        public Game Game;
        
        
        public Button(Game game, int x, int y, Texture2D texture) : base(game)
        {
            X = x;
            Y = y;
            Texture = texture;
            Game = game;
        }

        public bool HasMouseOn()
        {
            var mouse = Mouse.GetState();
            if (mouse.X > X && mouse.X < X + Texture.Width && mouse.Y > Y && mouse.Y < Y + Texture.Height) {
                return true;
            }
            return false;
        }

    }

    public class GameField : Button
    {
        public int GameX;
        public int GameY;
        public int GameZ;
        public Color HighlightColor { get; }

        private Color yellow = new Color(255, 250, 205); // lemonchiffon
        
        private bool highligted;
        private MouseState lastMouseState;
        
        public GameField(Game game, int x, int y, Texture2D texture, int gamePosX, int gamePosY, int gamePosZ) : base(game, x, y, texture)
        {
            HighlightColor = yellow;
            highligted = false;
            lastMouseState = Mouse.GetState();
            GameX = gamePosX;
            GameY = gamePosY;
            GameZ = gamePosZ;
        }
        
        public override void Update(GameTime gameTime)
        {
            var mouse = Mouse.GetState();
            if (HasMouseOn()) {
                highligted = true;
                if (lastMouseState.LeftButton == ButtonState.Released && mouse.LeftButton == ButtonState.Pressed) {
                    Game.PlaceSymbol(GameX, GameY, GameZ);
                }
            }
            else {
                highligted = false;
            }
            lastMouseState = mouse;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }

    public class HostingButton : Button
    {
        public HostingKind Kind;
        public HostingButton(Game game, int x, int y, Texture2D texture, HostingKind kind) : base(game, x, y, texture)
        {
            Kind = kind;
        }
    }
}