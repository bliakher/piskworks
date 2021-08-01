using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace piskworks
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;
        public SpriteBatch SpriteBatch;

        public Player Player;


        public Game()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            

            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        public void StartGame(HostingKind hostingKind)
        {
            Player player;
            switch (hostingKind) {
                case HostingKind.Guest:
                    player = new GuestPlayer();
                    break;
                case HostingKind.Host:
                    var server = new Server();
                    break;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            

            BasicEffect basicEffect = new BasicEffect(GraphicsDevice);
            Matrix world = Matrix.CreateTranslation(0, 0, 0);
            Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 10), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 480f, 0.01f, 100f);

            basicEffect.World = world;
            basicEffect.View = view;
            basicEffect.Projection = projection;
            basicEffect.VertexColorEnabled = true;
            
            /*var p1 = new Vector3(0, 1, 0);
            var p2 = new Vector3(0, 1, 2);

            var (vertices, indexes) = GraphicUtils.MakeStraightLine(p1, p2, 0.1f, Color.Black);*/

            var viewer = new Viewer(new GameBoard(4), this);
            var (vertices, indexes) = viewer.CreateHorizontalLines();
            
            VertexBuffer vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), vertices.Count,
                BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices.ToArray());
            
            IndexBuffer indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, indexes.Count, BufferUsage.WriteOnly);
            indexBuffer.SetData(indexes.ToArray());
            
            GraphicsDevice.SetVertexBuffer(vertexBuffer);
            GraphicsDevice.Indices = indexBuffer;
 
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;
 
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,0, indexes.Count / 3 );
            }
            
            base.Draw(gameTime);
        }
        
    }
}
