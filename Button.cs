using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace piskworks
{
    public class Button : DrawableGameComponent
    {
        public int X { get;  }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }
        public string Label { get; }
        public bool isHighlighted { get; set; }
        public Texture2D Texture { get; }
        public Game Game;
        
        
        public Button(Game game, int x, int y, int width, int height, string label, Texture2D texture = null) : base(game)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Texture = texture;
            Game = game;
            Label = label;
            isHighlighted = false;
        }

        public bool HasMouseOn()
        {
            var mouse = Mouse.GetState();
            if (mouse.X > X && mouse.X < X + Width && mouse.Y > Y && mouse.Y < Y + Height) {
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
        
        public GameField(Game game, int x, int y, int width, int height, string label, int gamePosX, int gamePosY, int gamePosZ, Texture2D texture = null) : base(game, x, y, width, height, label, texture)
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
                    //Game.PlaceSymbol(GameX, GameY, GameZ); // ToDo
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
        public HostingButton(Game game, int x, int y, int width, int height, string label, HostingKind kind, Texture2D texture = null) : base(game, x, y, width, height, label, texture)
        {
            Kind = kind;
        }
    }
}