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
        private float _rotationAngleZ;
        private float _rotationAngleX;

        private const float lineThickness = 0.02f;
        private VertexBuffer _boardLinesVertices;
        private IndexBuffer _boardLinesIndices;
        

        public Vizualizer3D(GameBoard board, Game game)
        {
            _board = board;
            //_board.FillForTesting();
            _game = game;
        }

        public void Draw()
        {
            Draw3DVizualization();
        }
        

        public void Draw3DVizualization()
        {
            drawBoardLines();
            DrawNoughts();
        }

        public void UpdateView(float rotateX, float rotateY)
        {
            // ToDo: fix left and right rotation, add up and down rotation
            
            // view matrix makes the cube the center of the screen 
            // camera position - far enough that the cube fits the screen
            // target - center of the cube
            // up - z axis
            var center = getBoardCenter();
            var cameraPos = _camera?.CameraPosition ?? new Vector3(0, 7, -5);
            cameraPos = Vector3.Transform(cameraPos, getRotationZ(rotateX));
            var view = Matrix.CreateLookAt(cameraPos, center, new Vector3(0, 1, 0));
            // projection - perspective with aspect ratio of the viewport
            var viewport = _game.GraphicsDevice.Viewport;
            var projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 
                viewport.Width / (float)viewport.Height, 0.01f, 100f);
            // make the cube smaller
            var scaleDown = Matrix.CreateScale(0.5f);
            // move down from the center - on y axis in screen space
            var moveDown = Matrix.CreateTranslation(new Vector3(0, -0.4f, 0));
            // additional changes applied after the projection - multiplied from right
            projection = projection * scaleDown * moveDown;
            _camera = new Camera(cameraPos, view, projection);
        }

        private Matrix getRotationZ(float rotationPercentage)
        {
            var maxRotation = 90;
            var rotation = maxRotation * rotationPercentage;
            return Matrix.CreateRotationZ(MathHelper.ToRadians(rotation));
        }

        // center in the vertex buffer from CreateBoardLines()
        // one corner is at (0,0,0) and the oposite at (N, N, N)
        // where N is the dimension of the board
        private Vector3 getBoardCenter()
        {
            return new Vector3(_board.N / 2f, _board.N / 2f, _board.N / 2f);
        }
        
        private void drawBoardLines()
        {
            if (_boardLinesVertices == null || _boardLinesIndices == null) {
                var (vertices, indices) = CreateBoardLines();
                _boardLinesVertices = GetVertexBuffer(vertices);
                _boardLinesIndices = GetIndexBuffer(indices);
            }
            var world = Matrix.CreateTranslation(new Vector3(0, 0, 0)); // cube is the world - it isn't moved in the world
            DrawVertexIndexBuffer(_boardLinesVertices, _boardLinesIndices, world, _camera.View, _camera.Projection);
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

        private void DrawNoughts()
        {
            var (vertices, indices) = CreateNought();
            var vertexBuffer = GetVertexBuffer(vertices);
            var indexBuffer = GetIndexBuffer(indices);
            
            var world = Matrix.CreateTranslation(new Vector3(0, 0, 0));
            DrawVertexIndexBuffer(vertexBuffer, indexBuffer, world, _camera.View, _camera.Projection);
            
            //DrawSymbols(gameTime, vertexBuffer, indexBuffer, SymbolKind.Nought);
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
        public Vector3 CameraPosition;
        public Matrix View;
        public Matrix Projection;

        public Camera(Vector3 cameraPosition, Matrix view, Matrix projection)
        {
            CameraPosition = cameraPosition;
            View = view;
            Projection = projection;
        }
    }
}