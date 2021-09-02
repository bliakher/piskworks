using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace piskworks
{
    public class Button
    {
        public int X { get; private set; }
        public int Y { get; private set;}
        public int Width { get; private set;}
        public int Height { get; private set;}
        public string Label { get; private set;}
        public bool IsHighlighted { get; set; }
        public Texture2D Texture { get; private set;}
        
        protected MouseState lastMouseState;

        public Button(Game game, int x, int y, int width, int height, string label, Texture2D texture = null)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Texture = texture;
            Label = label;
            IsHighlighted = false;
            
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

        public bool WasReleased()
        {
            var released = false;
            var mouse = Mouse.GetState();
            if (HasMouseOn()) {
                if (lastMouseState.LeftButton == ButtonState.Pressed && mouse.LeftButton == ButtonState.Released) {
                    released = true;
                }
            }
            lastMouseState = mouse;
            return released;
        }
        
        public void UpdateData(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public void UpdateLabel(string newLabel)
        {
            Label = newLabel;
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

    public class MouseTracker : Button
    {
        private int _mouseStartX;
        private int _mouseStartY;
        private bool _isPressedDown;

        private int mouseDistanceX;
        private int mouseDistanceY;
        public bool MovementRegistered { get; private set; }

        public MouseTracker(Game game, int x, int y, int width, int height, string label, Texture2D texture = null) : base(game, x, y, width, height, label, texture)
        {
            _mouseStartX = -1;
            _mouseStartY = -1;
            _isPressedDown = false;
        }

        public void Update()
        {
            IsHighlighted = HasMouseOn();
            if (_isPressedDown) {
                if (WasReleased()) {
                    _isPressedDown = false;
                    MovementRegistered = true;
                    mouseDistanceX = _mouseStartX - lastMouseState.X;
                    mouseDistanceY = _mouseStartY - lastMouseState.Y;
                    _mouseStartX = -1;
                    _mouseStartY = -1;
                }
            }
            else if (WasPresed()) {
                _isPressedDown = true;
                _mouseStartX = lastMouseState.X;
                _mouseStartY = lastMouseState.Y;
            }
        }

        public (float movX, float movY) GetMouseMovement()
        {
            if (!MovementRegistered) {
                return (0,0);
            }
            MovementRegistered = false; // reading is destructive - can read only once
            return (mouseDistanceX / (float)Width, mouseDistanceY / (float)Height);
        }
    }

    public class TextInput : Button
    {
        public bool InFocus;
        public TextInput(Game game, int x, int y, int width, int height, string label, Texture2D texture = null) : base(game, x, y, width, height, label, texture)
        {
            InFocus = false;
        }

       
    }
}