using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace piskworks
{
    public abstract class GameScreen
    {
        private Game _game;
        public Texture2D WhitePixel;
        public Rectangle WhitePixelSourceRect;
        public readonly Color PiskRed;
        public readonly Color PiskBlue;
        public readonly Color PiskDarkBlue;
        public readonly Color PiskBeige;

        protected GameScreen(Game game)
        {
            _game = game;
            WhitePixel = new Texture2D(_game.GraphicsDevice, 1, 1);
            WhitePixel.SetData(new []{Color.White});
            WhitePixelSourceRect = new Rectangle(0, 0, 1, 1);
            PiskBlue = new Color(0, 87, 132);
            PiskDarkBlue = new Color(4, 68, 103);
            PiskRed = new Color(167, 29, 38);
            PiskBeige = new Color(229, 224, 194);
        }

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(GameTime gameTime);

    }
    public class IntroScreen : GameScreen
    {
        private Game _game;
        private HostingButton _hostButton;
        private HostingButton _joinButton;

        private List<HostingButton> _buttonList;

        public IntroScreen(Game game) : base(game)
        {
            _game = game;

            createButtons();
        }

        public void DisplayIntro()
        {
            
        }

        private void createButtons()
        {
            var viewPort = _game.GraphicsDevice.Viewport;

            var widthPart = 7;
            var buttonWidth = viewPort.Width / widthPart;
            var buttonHeight = viewPort.Height / 20;
            var buttonOffset = viewPort.Height / 40;
            var buttonOffsetTop = viewPort.Height / 3;
            var buttonOffsetLeft = (widthPart - 1) / 2 * viewPort.Width / widthPart;

            _hostButton = new HostingButton(_game, buttonOffsetLeft, buttonOffsetTop, buttonWidth, buttonHeight, "Host", HostingKind.Host);
            _joinButton = new HostingButton(_game, buttonOffsetLeft, buttonOffsetTop + buttonHeight + buttonOffset, buttonWidth, buttonHeight, "Join", HostingKind.Guest);
            _buttonList = new List<HostingButton>() {_hostButton, _joinButton};
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var button in _buttonList) {
                button.isHighlighted = button.HasMouseOn();
                if (button.isHighlighted) {
                    Console.WriteLine(button.Label, "mouse on");
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            _game.GraphicsDevice.Clear(PiskBeige);

            var sb = _game.SpriteBatch;
            var viewPort = _game.GraphicsDevice.Viewport;

            sb.Begin(samplerState: SamplerState.PointClamp);

            foreach (var button in _buttonList) {
                var center = new Vector2(button.X + button.Width / 2, button.Y + button.Height / 2);
                var buttonColor = button.isHighlighted ? PiskDarkBlue : PiskBlue;
                
                sb.Draw(WhitePixel, new Rectangle(button.X, button.Y, button.Width, button.Height), 
                    WhitePixelSourceRect, buttonColor);
                sb.DrawStringCentered(button.Label, center, 2, Color.White);
            }
            
            sb.End();
        }
        
    }
}