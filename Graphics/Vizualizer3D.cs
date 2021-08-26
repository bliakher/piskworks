using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace piskworks
{
    public class Vizualizer3D
    {
        private GameBoard _board;
        private Game _game;
        private Camera _camera;
        private Matrix _world;
        private float _rotationAngleZ;
        private float _rotationAngleX;

        private const float lineThickness = 0.02f;

        public Vizualizer3D(GameBoard board, Game game)
        {
            _board = board;
            //_board.FillForTesting();
            _game = game;
            var view = Matrix.CreateLookAt(new Vector3(0, 0, 10), new Vector3(0, 0, 0), new Vector3(0, 0, 1));
            var projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 480f, 0.01f, 100f);
            _camera = new Camera(view, projection);
            _world = Matrix.CreateTranslation(new Vector3(0, 0, 0));
        }

        public void Draw(GameTime gameTime)
        {
            
        }

        public void DrawBoard()
        {
            
        }

        public void Draw3DVizualization(GameTime gameTime)
        {
            _rotationAngleZ = GetRotationFromTime(gameTime);
            UpdateView(gameTime);
            DrawBoardLines(gameTime);
            DrawNoughts(gameTime);
            DrawCrosses(gameTime);
        }

        public void UpdateView(GameTime gameTime)
        {
            var toBoardCenter = Matrix.CreateTranslation(new Vector3(_board.N / 2, _board.N / 2, _board.N / 2));
            var rotationZ = Matrix.CreateRotationZ(_rotationAngleZ);
            var rotationX = Matrix.CreateRotationX(_rotationAngleX);
            var lookAt = Matrix.CreateLookAt(new Vector3(0, 0, 10), new Vector3(0, 0, 0), new Vector3(0, 0, 1));
            _camera.View = toBoardCenter * rotationZ * rotationX * lookAt;
        }

        private void DrawVertexIndexBuffer(VertexBuffer vertices, IndexBuffer indices, Matrix world, Matrix view, Matrix projection)
        {
            BasicEffect basicEffect = new BasicEffect(_game.GraphicsDevice);
            basicEffect.World = world;
            basicEffect.View = view;
            basicEffect.Projection = projection;
            basicEffect.VertexColorEnabled = true;
            
            _game.GraphicsDevice.SetVertexBuffer(vertices);
            _game.GraphicsDevice.Indices = indices;
            
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            _game.GraphicsDevice.RasterizerState = rasterizerState;
            
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,0, indices.IndexCount / 3 );
            }
        }

        private VertexBuffer GetVertexBuffer(List<VertexPositionColor> vertices)
        {
            VertexBuffer vertexBuffer = new VertexBuffer(_game.GraphicsDevice, typeof(VertexPositionColor), vertices.Count,
                BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices.ToArray());
            return vertexBuffer;
        }

        private IndexBuffer GetIndexBuffer(List<int> indices)
        {
            IndexBuffer indexBuffer = new IndexBuffer(_game.GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices.ToArray());
            return indexBuffer;
        }

        private float GetRotationFromTime(GameTime gameTime)
        {
            //return Matrix.CreateRotationY(MathHelper.ToRadians(gameTime.TotalGameTime.Seconds % 360));
            return MathHelper.ToRadians(gameTime.TotalGameTime.Seconds % 360);
        }
        
        private void DrawBoardLines(GameTime gameTime)
        {
            var (vertices, indices) = CreateBoardLines();
            var vertexBuffer = GetVertexBuffer(vertices);
            var indexBuffer = GetIndexBuffer(indices);
            
            DrawVertexIndexBuffer(vertexBuffer, indexBuffer, _world, _camera.View, _camera.Projection);
        }

        private void DrawNoughts(GameTime gameTime)
        {
            var (vertices, indices) = CreateNought();
            var vertexBuffer = GetVertexBuffer(vertices);
            var indexBuffer = GetIndexBuffer(indices);
            DrawSymbols(gameTime, vertexBuffer, indexBuffer, SymbolKind.Nought);
        }
        private void DrawCrosses(GameTime gameTime)
        {
            var (vertices, indices) = CreateCross();
            var vertexBuffer = GetVertexBuffer(vertices);
            var indexBuffer = GetIndexBuffer(indices);
            DrawSymbols(gameTime, vertexBuffer, indexBuffer, SymbolKind.Cross);
        }

        private void DrawSymbols(GameTime gameTime, VertexBuffer vertexBuffer, IndexBuffer indexBuffer,
            SymbolKind symbol)
        {
            for (int x = 0; x < _board.N; x++) {
                for (int y = 0; y < _board.N; y++) {
                    for (int z = 0; z < _board.N; z++) {
                        if (_board.GetSymbol(x, y, z) == symbol) {
                            var inBoard = Matrix.CreateTranslation(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
                            DrawVertexIndexBuffer(vertexBuffer, indexBuffer, inBoard, _camera.View, _camera.Projection);
                        }
                    }
                }
            }
        } 

        public (List<VertexPositionColor>, List<int>) CreateBoardLines()
        {
            List<VertexPositionColor> vertices = new List<VertexPositionColor>();
            List<int> indices = new List<int>();

            for (int z = 0; z <= _board.N; z++) {
                for (int x = 0; x <= _board.N; x++) {
                    var p1 = new Vector3(x, 0, z);
                    var p2 = new Vector3(x, _board.N, z);
                    var (v, i) = GraphicUtils.MakeStraightLine(p1, p2, lineThickness, Color.Black, vertices.Count);
                    vertices.AddRange(v);
                    indices.AddRange(i);
                }
                for (int y = 0; y <= _board.N; y++) {
                    var p1 = new Vector3(0, y, z);
                    var p2 = new Vector3(_board.N, y, z);
                    var (v, i) = GraphicUtils.MakeStraightLine(p1, p2, lineThickness, Color.Black, vertices.Count);
                    vertices.AddRange(v);
                    indices.AddRange(i);
                }
            }
            for (int y = 0; y <= _board.N; y++) {
                for (int x = 0; x <= _board.N; x++) {
                    var p1 = new Vector3(x, y, 0);
                    var p2 = new Vector3(x, y, _board.N);
                    var (v, i) = GraphicUtils.MakeStraightLine(p1, p2, lineThickness, Color.Black, vertices.Count);
                    vertices.AddRange(v);
                    indices.AddRange(i);
                }
            }
            return (vertices, indices);
        }

        public (List<VertexPositionColor>, List<int>) CreateNought()
        {
            Color col = Color.Red;

            var a = new VertexPositionColor(new Vector3(-1 / 3, 0, -1 / 3), col);
            var b = new VertexPositionColor(new Vector3(1 / 3, 0, -1 / 3), col);
            var c = new VertexPositionColor(new Vector3(-1 / 3, 0, 1 / 3), col);
            var d = new VertexPositionColor(new Vector3(1 / 3, 0, 1 / 3), col);
            
            List<VertexPositionColor> vertices = new List<VertexPositionColor>(){a, b, c, d};
            List<int> indices = new List<int>(){0, 1, 2, 1, 2, 3};
            
            return (vertices, indices);
        }
        public (List<VertexPositionColor>, List<int>) CreateCross()
        {
            Color col = Color.Blue;

            var a = new VertexPositionColor(new Vector3(-1 / 3, 0, -1 / 3), col);
            var b = new VertexPositionColor(new Vector3(1 / 3, 0, -1 / 3), col);
            var c = new VertexPositionColor(new Vector3(-1 / 3, 0, 1 / 3), col);
            var d = new VertexPositionColor(new Vector3(1 / 3, 0, 1 / 3), col);
            
            List<VertexPositionColor> vertices = new List<VertexPositionColor>(){a, b, c, d};
            List<int> indices = new List<int>(){0, 1, 2, 1, 2, 3};
            
            return (vertices, indices);
        }
    }

    public class Camera
    {
        public Matrix View;
        public Matrix Projection;

        public Camera(Matrix view, Matrix projection)
        {
            View = view;
            Projection = projection;
        }
    }
}