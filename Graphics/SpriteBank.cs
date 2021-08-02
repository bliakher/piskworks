using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace piskworks
{
    public class Sprite {

        public Texture2D Texture { get; set; }
        public Rectangle SourceRect {
            get => sourceRect;
            set {
                sourceRect = value;
                if (!originSet) {
                    origin = value.Size.ToVector2() / 2f;
                }
            }
        }
        private Rectangle sourceRect;
        bool originSet = false;

        public Vector2 Origin {
            get => origin;
            set {
                origin = value;
                originSet = true;
            }
        }

        private Vector2 origin = Vector2.Zero;

        public Vector2 UVTopLeft => new Vector2(SourceRect.X, SourceRect.Y) / new Vector2(Texture.Width, Texture.Height);
        public Vector2 UVTopRight => new Vector2(SourceRect.X + SourceRect.Width, SourceRect.Y) / new Vector2(Texture.Width, Texture.Height);
        public Vector2 UVBotLeft => new Vector2(SourceRect.X, SourceRect.Y + SourceRect.Height) / new Vector2(Texture.Width, Texture.Height);
        public Vector2 UVBotRight => new Vector2(SourceRect.X + SourceRect.Width, SourceRect.Y + SourceRect.Height) / new Vector2(Texture.Width, Texture.Height);
    }
    
    public class SpriteBank
    {
        public Texture2D SourceTexture;

        public Sprite GameField;
        public Sprite Cross;
        public Sprite Nought;

        public SpriteBank(Texture2D sourceTexture)
        {
            SourceTexture = sourceTexture;

            GameField = new Sprite() {Texture = SourceTexture, SourceRect = new Rectangle(0, 0, 32, 32)};
            Cross = new Sprite() {Texture = SourceTexture, SourceRect = new Rectangle(32, 0, 32, 32)};
            Nought = new Sprite() {Texture = SourceTexture, SourceRect = new Rectangle(64, 0, 32, 32)};
        }
    }
}