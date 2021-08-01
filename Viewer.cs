using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace piskworks
{
    public class Viewer
    {
        private GameBoard _board;
        private Game _game;

        public Viewer(GameBoard board, Game game)
        {
            _board = board;
            _game = game;
        }

        public void Draw()
        {
            
        }

        public void DrawBoard()
        {
            
        }

        public void Draw3DVizualization()
        {
            
        }

        public (List<VertexPositionColor>, List<int>) CreateHorizontalLines()
        {
            List<VertexPositionColor> vertices = new List<VertexPositionColor>();
            List<int> indices = new List<int>();

            for (int z = 0; z < _board.N; z += _board.N - 1) {
                for (int x = 0; x < _board.N; x++) {
                    var p1 = new Vector3(x, 0, z);
                    var p2 = new Vector3(x, _board.N, z);
                    var (v, i) = GraphicUtils.MakeStraightLine(p1, p2, 0.1f, Color.Black, vertices.Count);
                    vertices.AddRange(v);
                    indices.AddRange(i);
                }
            }
            return (vertices, indices);
        } 
    }
}