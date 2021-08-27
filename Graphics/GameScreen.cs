using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace piskworks
{
    public abstract class GameScreen
    {
        public Game _game;
        public Texture2D WhitePixel;
        public Rectangle WhitePixelSourceRect;
        public readonly Color PiskRed;
        public readonly Color PiskBlue;
        public readonly Color PiskDarkBlue;
        public readonly Color PiskBeige;
        public readonly Color PiskHighlight;
        public readonly Color PiskWinCross;
        public readonly Color PiskWinNought;

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
            PiskHighlight = new Color(238, 225, 144);
            PiskWinCross = new Color(63, 134, 172);
            PiskWinNought = new Color(207, 105, 110);
        }

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(GameTime gameTime);

        protected void DrawButtons(IEnumerable<Button> buttons, SpriteBatch sb)
        {
            foreach (var button in buttons) {
                DrawButton(button, sb);
            }
        }
        protected void DrawButton(Button button, SpriteBatch sb)
        {
            var center = new Vector2(button.X + button.Width / 2, button.Y + button.Height / 2);
            var buttonColor = button.isHighlighted ? PiskDarkBlue : PiskBlue;
                
            sb.Draw(WhitePixel, new Rectangle(button.X, button.Y, button.Width, button.Height), 
                WhitePixelSourceRect, buttonColor);
            sb.DrawStringCentered(button.Label, center, 2, Color.White);
        }

    }
    public class IntroScreen : GameScreen
    {
        private HostingButton _hostButton;
        private HostingButton _joinButton;

        private List<HostingButton> _buttonList;

        public IntroScreen(Game game) : base(game)
        {
            createOrUpdateButtons(true);
        }

        private void createOrUpdateButtons(bool create = false)
        {
            var viewPort = _game.GraphicsDevice.Viewport;

            var widthPart = 7;
            var buttonWidth = viewPort.Width / widthPart;
            var buttonHeight = viewPort.Height / 20;
            var buttonOffset = viewPort.Height / 40;
            var buttonOffsetTop = 2 * viewPort.Height / 3;
            var buttonOffsetLeft = (widthPart - 1) / 2 * viewPort.Width / widthPart;

            if (create) {
                _hostButton = new HostingButton(_game, buttonOffsetLeft, buttonOffsetTop, 
                    buttonWidth, buttonHeight, "Host", HostingKind.Host);
                _joinButton = new HostingButton(_game, buttonOffsetLeft, buttonOffsetTop + buttonHeight + buttonOffset,
                    buttonWidth, buttonHeight, "Join", HostingKind.Guest);
                _buttonList = new List<HostingButton>() {_hostButton, _joinButton};
            }
            else {
                _hostButton.UpdateData(buttonOffsetLeft, buttonOffsetTop, buttonWidth, buttonHeight, "Host");
                _joinButton.UpdateData(buttonOffsetLeft, buttonOffsetTop + buttonHeight + buttonOffset, 
                    buttonWidth, buttonHeight, "Join");
            }
           
        }

        public override void Update(GameTime gameTime)
        {
            createOrUpdateButtons();
            foreach (var button in _buttonList) {
                button.isHighlighted = button.HasMouseOn();
                if (button.WasPresed()) {
                    _game.TransitionFromIntro(button.Kind);
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            _game.GraphicsDevice.Clear(PiskBeige);

            var sb = _game.SpriteBatch;
            var viewPort = _game.GraphicsDevice.Viewport;

            sb.Begin(samplerState: SamplerState.PointClamp);

            var logoOffset = viewPort.Height / 7;
            sb.DrawStringCentered("PISK", new Vector2(viewPort.Width / 2, viewPort.Height / 3), 8, PiskRed);
            sb.DrawStringCentered("WORKS", new Vector2(viewPort.Width / 2, viewPort.Height / 3 + logoOffset), 6, PiskBlue);

            DrawButtons(_buttonList, sb);
            
            sb.End();
        }
    }

    public class DimensionScreen : GameScreen
    {
        private List<NumberButton> _buttonList;
        public DimensionScreen(Game game) : base(game)
        {
            _buttonList = new List<NumberButton>();
            createOrUpdateButtons(true);
        }

        private void createOrUpdateButtons(bool create = false)
        {
            var viewPort = _game.GraphicsDevice.Viewport;
            
            var widthPart = 6;
            var buttonWidth = viewPort.Width / widthPart;
            var buttonHeight = viewPort.Height / 20;
            var buttonOffset = viewPort.Height / 40;
            var buttonOffsetTop = viewPort.Height / 3;
            var buttonOffsetLeft = (viewPort.Width - buttonWidth ) / 2;

            if (create) {
                for (int i = 3; i <= 6; i++) {
                    var butNum = i - 3;
                    var b = new NumberButton(_game, buttonOffsetLeft, buttonOffsetTop + butNum * buttonHeight + butNum * buttonOffset,
                        buttonWidth, buttonHeight, $"{i} x {i} x {i}", i);
                    _buttonList.Add(b);
                }
            }
            else {
                for (int i = 3; i <= 6; i++) {
                    var butNum = i - 3;
                    _buttonList[butNum].UpdateData(buttonOffsetLeft, buttonOffsetTop + butNum * buttonHeight + butNum * buttonOffset,
                        buttonWidth, buttonHeight, $"{i} x {i} x {i}");
                }
            }
            
        }

        public override void Update(GameTime gameTime)
        {
            createOrUpdateButtons();
            foreach (var button in _buttonList) {
                button.isHighlighted = button.HasMouseOn();
                if (button.WasPresed()) {
                    _game.TransitionFromDimension(button.Number);
                    break;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            _game.GraphicsDevice.Clear(PiskBeige);

            var viewPort = _game.GraphicsDevice.Viewport;
            var sb = _game.SpriteBatch;

            sb.Begin(samplerState: SamplerState.PointClamp);
            sb.DrawStringCentered("Choose dimension:", new Vector2(viewPort.Width / 2, viewPort.Height / 5), 2, PiskBlue);
            DrawButtons(_buttonList, sb);
            sb.End();
        }
    }

    public class WaitScreen : GameScreen
    {
        private const string msg = "Waiting for the other player to connect..";
        private Button _cancelButton;
        public WaitScreen(Game game) : base(game)
        {
            createOrUpdateButtons(true);
        }

        private void createOrUpdateButtons(bool create = false)
        {
            var viewport = _game.GraphicsDevice.Viewport;
            var centedWidth = viewport.Width / 2;
            var buttonWidth = viewport.Width / 7;
            var buttonHeight = viewport.Height / 10;
            var buttonLabel = "Cancel";
            
            if (create) {
                _cancelButton = new Button(_game, centedWidth - buttonWidth / 2, 2 * viewport.Height / 3, buttonWidth,
                    buttonHeight, buttonLabel);
            } else {
                _cancelButton.UpdateData(centedWidth - buttonWidth / 2, 2 * viewport.Height / 3, buttonWidth,
                    buttonHeight, buttonLabel);
            }
        }

        public override void Update(GameTime gameTime)
        {
            createOrUpdateButtons();
            if (_cancelButton.WasPresed()) {
                _game.RestartGame();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            _game.GraphicsDevice.Clear(PiskBeige);

            var sb = _game.SpriteBatch;
            var viewport = _game.GraphicsDevice.Viewport;
            var centedWidth = viewport.Width / 2;
            
            sb.Begin(samplerState: SamplerState.PointClamp);
            sb.DrawStringCentered(msg, new Vector2(centedWidth, viewport.Height / 2), 2, PiskBlue);
            DrawButton(_cancelButton, sb);
            sb.End();
        }
    }

    public class PlayScreen : GameScreen
    {
        private Vizualizer3D _vizualizer;
        private GameBoard _board;
        private GameField[,,] _fieldList;
        private Button _menuButton;
        
        private bool _itsMyTurn;

        private bool fieldsMarkedFlag;
        
        public PlayScreen(Game game, GameBoard board, bool itsMyTurn) : base(game)
        {
            _board = board;
            _vizualizer = new Vizualizer3D(_board, _game);
            _fieldList = new GameField[board.N, board.N, board.N];
            _itsMyTurn = itsMyTurn;
            fieldsMarkedFlag = false;
        }

        private void checkFields()
        {
            foreach (var field in _fieldList) {
                if (field == null) {
                    return;
                }
                field.isHighlighted = field.HasMouseOn() && _board.GetSymbol(field.GameX, field.GameY, field.GameZ) == SymbolKind.Free;
                if (field.WasPresed()) {
                    _game.Player.DoMove(field.GameX, field.GameY, field.GameZ);
                    return;
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (_game.IsGameOver) {
                if (!fieldsMarkedFlag) {
                    markWinningFields();
                    fieldsMarkedFlag = true;
                }
                updateMenuButton();
                if (_menuButton.WasPresed()) {
                    _game.RestartGame();
                }
            }
            else { // normal game play
                _itsMyTurn = !_game.Player.WaitingForResponse;
                if (_itsMyTurn) {
                    checkFields();
                }
                else if (_game.Player.Comunicator.IsMsgAvailable()) {
                    _game.Player.DealWithMsg();
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            var viewport = _game.GraphicsDevice.Viewport;
            var sb = _game.SpriteBatch;
            
            _game.GraphicsDevice.Clear(PiskBeige);

            var layerSpacing = viewport.Width / 20;
            var layersTopLeftOffset = viewport.Height / 10;
            var layerSide = (viewport.Width - (_board.N - 1) * layerSpacing - layersTopLeftOffset * 2) / _board.N;
            var fieldSize = layerSide / _board.N;

            var textTopOffset = viewport.Height / 30;
            var whosPlayingText = _itsMyTurn ? "It's YOUR turn!" : "Waiting for oponent's move";
            var whoWonText = _game.ThisPlayerWon ? "You WON!" : "You LOST..";
            var displayedText = _game.IsGameOver ? whoWonText : whosPlayingText;
            var textColor = _game.IsGameOver ? PiskRed : PiskBlue;

            SpriteBank bank = _game.SpriteBank;
            
            sb.Begin(samplerState: SamplerState.PointClamp);
            
            sb.DrawStringCentered(displayedText, new Vector2(viewport.Width / 2, textTopOffset), 2, PiskBlue);
            if (_game.IsGameOver) {
                DrawButton(_menuButton, sb);
            }

            for (int z = 0; z < _board.N; z++) {

                var leftCornerX = layersTopLeftOffset + z * layerSide + z * layerSpacing;
                var leftCornerY = layersTopLeftOffset;
                for (int x = 0; x < _board.N; x++) {
                    var xScreen = leftCornerX + x * fieldSize;
                    for (int y = 0; y < _board.N; y++) {
                        var yScreen = leftCornerY + y * fieldSize;

                        if (_fieldList[x, y, z] == null) { // create buttons on first draw
                            var field = new GameField(_game, xScreen, yScreen, fieldSize, fieldSize, 
                                null, x, y, z, bank.GameField.Texture);
                            _fieldList[x, y, z] = field;
                        }
                        else { // update button size - resizing
                            _fieldList[x, y, z].UpdateData(xScreen, yScreen, fieldSize, fieldSize, null);
                        }
                        // get correct highlight for fields
                        Color fieldHighlight;
                        if (_game.IsGameOver) {
                            fieldHighlight = _fieldList[x, y, z].HasWinningSymbol ? getWinHighlightColor() : Color.White;
                        }
                        else {
                            fieldHighlight = _fieldList[x, y, z].isHighlighted ? PiskHighlight : Color.White;
                        }

                        var fieldRect = new Rectangle(xScreen, yScreen, fieldSize,
                            fieldSize);
                        
                        sb.Draw(bank.GameField.Texture, fieldRect, bank.GameField.SourceRect, fieldHighlight );
                        
                        var symbol = _board.GetSymbol(x, y, z);
                        if (symbol == SymbolKind.Cross) {
                            sb.Draw(bank.Cross.Texture, fieldRect, bank.Cross.SourceRect, Color.White);
                        }
                        else if (symbol == SymbolKind.Nought) {
                            sb.Draw(bank.Nought.Texture, fieldRect, bank.Nought.SourceRect, Color.White);
                        }
                    }
                }
            }
            
            sb.End();
        }
        private void markWinningFields()
        {
            if (_game.Board.WinningFields == null) {
                return;
            }
            foreach (var field in _game.Board.WinningFields) {
                _fieldList[field.X, field.Y, field.Z].HasWinningSymbol = true;
            }
        }

        private Color getWinHighlightColor()
        {
            SymbolKind winSymbol;
            if (_game.ThisPlayerWon) {
                winSymbol = _game.Player.PlayerSymbol;
            }
            else {
                winSymbol = _game.Player.PlayerSymbol == SymbolKind.Cross ? SymbolKind.Nought : SymbolKind.Cross;
            }
            return  winSymbol == SymbolKind.Cross ? PiskWinCross : PiskWinNought;
        }

        private void updateMenuButton()
        {
            var viewport = _game.GraphicsDevice.Viewport;
            var screenX = 2 * viewport.Width / 3;
            var screenY = viewport.Height / 30;
            var height = viewport.Height / 20;
            var width = viewport.Width / 8;
            var label = "Menu";
            
            if (_menuButton == null) {
                _menuButton = new Button(_game, screenX, screenY, width, height, label);
            }
            else {
                _menuButton.UpdateData(screenX, screenY, width, height, label);
            }
        }
    }
    
}