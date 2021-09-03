using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using piskworks.GameSrc;
using piskworks.Graphics;
using Game = piskworks.GameSrc.Game;

namespace piskworks.Utils
{
    /// <summary>
    /// Button. Element that can be placed on the <see cref="GameScreen"/>
    /// It has dimensions and the coordinates of placement on the screen
    /// </summary>
    public class Button
    {
        /// <summary>
        /// X coordinate of the uper left corner on the screen
        /// </summary>
        public int X { get; private set; }
        /// <summary>
        ///  Y coordinate of the uper left corner on the screen
        /// </summary>
        public int Y { get; private set;}
        /// <summary>
        /// Width of the button
        /// </summary>
        public int Width { get; private set;}
        /// <summary>
        /// Height of the button
        /// </summary>
        public int Height { get; private set;}
        /// <summary>
        /// Text written on the button
        /// </summary>
        public string Label { get; private set;}
        /// <summary>
        /// Indicator if the button is highlihgted
        /// Button should be highlighted when the mouse is hovering above
        /// </summary>
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
            lastMouseState = Mouse.GetState();

        }

        /// <summary>
        /// True if the mouse hovers on the button.
        /// </summary>
        /// <returns></returns>
        public bool HasMouseOn()
        {
            var mouse = Mouse.GetState();
            if (mouse.X > X && mouse.X < X + Width && mouse.Y > Y && mouse.Y < Y + Height) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// True if button was pressed.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// True if a pressed button was just released.
        /// </summary>
        /// <returns></returns>
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
        
        /// <summary>
        /// Updates the dimensions and the placement of the button.
        /// </summary>
        public void UpdateData(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Update the label of the button.
        /// </summary>
        /// <param name="newLabel"></param>
        public void UpdateLabel(string newLabel)
        {
            Label = newLabel;
        }
        
    }

    /// <summary>
    /// Element that represents a field of a game board in 2D.
    /// It holds the coordinates of the field in the game board.
    /// </summary>
    public class GameField : Button
    {
        /// <summary>
        /// X coordinate
        /// </summary>
        public int GameX;
        /// <summary>
        /// Y coordinate
        /// </summary>
        public int GameY;
        /// <summary>
        /// Z coordinate
        /// </summary>
        public int GameZ;

        /// <summary>
        /// True if the field holds a symbol that is part of the winning line of symbols.
        /// </summary>
        public bool HasWinningSymbol;

        public GameField(Game game, int x, int y, int width, int height, string label, int gamePosX, int gamePosY, int gamePosZ, Texture2D texture = null) : base(game, x, y, width, height, label, texture)
        {
            GameX = gamePosX;
            GameY = gamePosY;
            GameZ = gamePosZ;
            HasWinningSymbol = false;
        }
    }

    /// <summary>
    /// Button with information on the type of <see cref="Player"/> role - host or guest.
    /// Used for player to decide on his role. 
    /// </summary>
    public class HostingButton : Button
    {
        /// <summary>
        /// Role of the player
        /// </summary>
        public HostingKind Kind;
        public HostingButton(Game game, int x, int y, int width, int height, string label, HostingKind kind, Texture2D texture = null) : base(game, x, y, width, height, label, texture)
        {
            Kind = kind;
        }
    }

    /// <summary>
    /// Button holding a number value.
    /// </summary>
    public class NumberButton : Button
    {
        public int Number;
        public NumberButton(Game game, int x, int y, int width, int height, string label, int number, Texture2D texture = null) : base(game, x, y, width, height, label, texture)
        {
            Number = number;
        }
    }

    /// <summary>
    /// Element that tracks the movement of the mouse.
    /// It registers a drag of the mouse above the tracker
    /// and counts the length of the drag in regards to the tracker surface.
    /// </summary>
    public class MouseTracker : Button
    {
        private int _mouseStartX;
        private int _mouseStartY;
        private bool _isPressedDown;

        private int mouseDistanceX;
        private int mouseDistanceY;
        /// <summary>
        /// True after a drag is registered but before the data is read by calling <see cref="GetMouseMovement"/>
        /// </summary>
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

    /// <summary>
    /// Text input field
    /// </summary>
    public class TextInput : Button
    {
        public bool InFocus;
        public TextInput(Game game, int x, int y, int width, int height, string label, Texture2D texture = null) : base(game, x, y, width, height, label, texture)
        {
            InFocus = false;
        }

       
    }
}