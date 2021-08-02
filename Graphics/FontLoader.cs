using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace piskworks
{
    public class Tileset
    {
        public int TileWidth { get; private set; }
        public int TileHeight { get; private set; }
        public Texture2D Texture { get; private set; }

        public Tileset(Texture2D texture, int tileWidth, int tileHeight)
        {
            Texture = texture;
            TileWidth = tileWidth;
            TileHeight = tileHeight;
        }

        public Rectangle GetTile(int id)
        {
            int W = Texture.Width / TileWidth;

            return new Rectangle((id % W) * TileWidth, (id / W) * TileHeight, TileWidth, TileHeight);
        }
    }
    
    public static class FontLoader {
        public static SpriteFont CreateFont(Texture2D srcTexture) {
            var glyphBounds = new List<Rectangle>();
            var cropping = new List<Rectangle>();
            var characters = new List<char>();
            var kernings = new List<Vector3>();

            Tileset fontTileset = new Tileset(srcTexture, 7, 7);

            // the font in the tileset begins at tile no. 256 with space (' ')
            // so we start creating the spritefont from there
            for (char c = ' '; c <= '~'; c++)
            {
                glyphBounds.Add(fontTileset.GetTile(c - ' '));
                cropping.Add(new Rectangle(0, 0, 7, 7));
                characters.Add(c);
                kernings.Add(new Vector3(0, 7, 0));
            }

            // !!! following line needs MonoGame >=3.7, because in older versions SpriteFont constructor is private
            return new SpriteFont(srcTexture, glyphBounds, cropping, characters, fontTileset.TileHeight, 0f, kernings, '?');
        }
    }
}