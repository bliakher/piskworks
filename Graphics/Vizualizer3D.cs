using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace piskworks
{
    public class Vizualizer3D
    {
        private GameBoard _board;
        private Game _game;
        private Camera _camera;

        private const float lineThickness = 0.02f;
        private Model3D _boardLines;
        private BasicEffect _basicEffect;
        

        public Vizualizer3D(GameBoard board, Game game)
        {
            _board = board;
            //_board.FillForTesting();
            _game = game;
            _basicEffect = new BasicEffect(_game.GraphicsDevice);
        }

        public void Draw()
        {
            Draw3DVizualization();
        }
        

        public void Draw3DVizualization()
        {
            DrawNoughts();
            DrawCrosses();
            drawBoardLines();
        }

        public void UpdateView(float rotateX, float rotateY)
        {
            // ToDo: fix left and right rotation, add up and down rotation
            
            // view matrix makes the cube the center of the screen 
            // camera position - far enough that the cube fits the screen
            // target - center of the cube
            // up - z axis
            var center = getBoardCenter();
            var cameraPos = _camera?.CameraPosition ?? new Vector3(2.5f, 7, 8);
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
            if (_boardLines == null) {
                var (vertices, indices) = CreateBoardLines();
                _boardLines = new Model3D(_game.GraphicsDevice, vertices, indices);
            }
            var world = Matrix.CreateTranslation(new Vector3(0, 0, 0)); // cube is the world - it isn't moved in the world
            DrawVertexIndexBuffer(_boardLines.VertexBuffer, _boardLines.IndexBuffer, world, _camera.View, _camera.Projection, new Vector3(0,0 ,0));
        }

        private void DrawVertexIndexBuffer(VertexBuffer vertices, IndexBuffer indices, Matrix world, Matrix view, Matrix projection, Vector3 color)
        {
            _basicEffect.World = world;
            _basicEffect.View = view;
            _basicEffect.Projection = projection;
            _basicEffect.VertexColorEnabled = true;
            //_basicEffect.DiffuseColor = color;
            
            _game.GraphicsDevice.SetVertexBuffer(vertices);
            _game.GraphicsDevice.Indices = indices;
            
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            _game.GraphicsDevice.RasterizerState = rasterizerState;
            
            foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,0, indices.IndexCount / 3 );
            }
        }
        

        private void DrawNoughts()
        {
            DrawSymbols(_game.Nougth, SymbolKind.Nought);    
        }
        private void DrawCrosses()
        {
            DrawSymbols(_game.Cross, SymbolKind.Cross);
        }

        private void DrawSymbols(Model3D model,
            SymbolKind symbol)
        {
            for (int x = 0; x < _board.N; x++) {
                for (int y = 0; y < _board.N; y++) {
                    for (int z = 0; z < _board.N; z++) {
                        if (_board.GetSymbol(x, y, z) == symbol) {
                            var scaleDown = Matrix.CreateScale(0.5f);
                            var inBoard = Matrix.CreateTranslation(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
                            var world = scaleDown * inBoard;
                            DrawVertexIndexBuffer(model.VertexBuffer, model.IndexBuffer, 
                                world, _camera.View, _camera.Projection, new Vector3(1, 1, 1));
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
                    var (v, i) = GraphicUtils.MakeStraightLine(p1, p2, lineThickness, Color.Red, vertices.Count);
                    vertices.AddRange(v);
                    indices.AddRange(i);
                }
            }
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