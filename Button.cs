using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace piskworks
{
    public class Button : DrawableGameComponent
    {
        public int X { get; private set; }
        public int Y { get; private set;}
        public int Width { get; private set;}
        public int Height { get; private set;}
        public string Label { get; private set;}
        public bool isHighlighted { get; set; }
        public Texture2D Texture { get; private set;}
        private Game Game;
        
        private MouseState lastMouseState;

        public void UpdateData(int x, int y, int width, int height, string label)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Label = label;
        }

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

        public bool WasPresed()
        {
            var pressed = false;
            var mouse = Mouse.GetState();
            if (HasMouseOn()) {
                if (lastMouseState.LeftButton == ButtonState.Released && mouse.LeftButton == ButtonState.Pressed) {
                    pressed = true;
                }
            }
            lastMouseState = mouse;
            return pressed;
        }
        
    }

    public class GameField : Button
    {
        public int GameX;
        public int GameY;
        public int GameZ;

        public bool HasWinningSymbol;

        public GameField(Game game, int x, int y, int width, int height, string label, int gamePosX, int gamePosY, int gamePosZ, Texture2D texture = null) : base(game, x, y, width, height, label, texture)
        {
            GameX = gamePosX;
            GameY = gamePosY;
            GameZ = gamePosZ;
            HasWinningSymbol = false;
        }
        
        public override void Update(GameTime gameTime)
        {
            
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

    public class NumberButton : Button
    {
        public int Number;
        public NumberButton(Game game, int x, int y, int width, int height, string label, int number, Texture2D texture = null) : base(game, x, y, width, height, label, texture)
        {
            Number = number;
        }
    }
}