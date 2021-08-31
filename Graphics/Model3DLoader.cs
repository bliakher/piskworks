using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace piskworks
{
    public class Model3D
    {
        public VertexBuffer VertexBuffer { get; private set; }
        public IndexBuffer IndexBuffer { get; private set; }

        public Model3D(GraphicsDevice graphicsDevice, IList<VertexPositionColor> vertices, IList<int> indices)
        {
            VertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), vertices.Count,
                BufferUsage.WriteOnly);
            VertexBuffer.SetData(vertices.ToArray());
            IndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.WriteOnly);
            IndexBuffer.SetData(indices.ToArray());
        }
        
        public Model3D(GraphicsDevice graphicsDevice, IList<VertexPosition> vertices, IList<int> indices)
        {
            VertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPosition), vertices.Count,
                BufferUsage.WriteOnly);
            VertexBuffer.SetData(vertices.ToArray());
            IndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.WriteOnly);
            IndexBuffer.SetData(indices.ToArray());
        }
    }
    
    public class Model3DLoader
    {
        private GraphicsDevice _graphicsDevice;

        public Model3DLoader(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        public Model3D Load(string sourceFileName)
        {
            var reader = new StreamReader(sourceFileName);
            var line = reader.ReadLine();
            List<VertexPosition> vertices = null;
            List<int> indices = null;
            while (line != null) {
                var segments = line.Split();
                if (segments[0] == "o") { // beginning of new model
                    vertices = new List<VertexPosition>();
                    indices = new List<int>();
                }
                else if (segments[0] == "v") { // vertex - v 0.123 0.234 0.345
                    var x = float.Parse(segments[1]);
                    var y = float.Parse(segments[2]);
                    var z = float.Parse(segments[3]);
                    vertices.Add(new VertexPosition(new Vector3(x, y, z)));
                }
                else if (segments[0] == "f") { // face - 3 vertex indexes - f v1/vt1/vn1 v2/vt2/vn2 v3/vt3/vn3
                    for (int i = 1; i < 4; i++) {
                        var indexTriplet = segments[i].Split("/");
                        var index = Int32.Parse(indexTriplet[0]) - 1; // in .obj indexes start at 1
                        indices.Add(index);
                    }
                }
                
                line = reader.ReadLine();
            }
            
            reader.Close();
            return new Model3D(_graphicsDevice, vertices, indices);
        }
        
        public Model3D LoadWithColor(string sourceFileName, Color color)
        {
            var reader = new StreamReader(sourceFileName);
            var line = reader.ReadLine();
            List<VertexPositionColor> vertices = null;
            List<int> indices = null;
            while (line != null) {
                var segments = line.Split();
                if (segments[0] == "o") { // beginning of new model
                    vertices = new List<VertexPositionColor>();
                    indices = new List<int>();
                }
                else if (segments[0] == "v") { // vertex - v 0.123 0.234 0.345
                    var x = float.Parse(segments[1]);
                    var y = float.Parse(segments[2]);
                    var z = float.Parse(segments[3]);
                    vertices.Add(new VertexPositionColor(new Vector3(x, y, z), color));
                }
                else if (segments[0] == "f") { // face - 3 vertex indexes - f v1/vt1/vn1 v2/vt2/vn2 v3/vt3/vn3
                    for (int i = 1; i < 4; i++) {
                        var indexTriplet = segments[i].Split("/");
                        var index = Int32.Parse(indexTriplet[0]) - 1; // in .obj indexes start at 1
                        indices.Add(index);
                    }
                }
                
                line = reader.ReadLine();
            }
            
            reader.Close();
            return new Model3D(_graphicsDevice, vertices, indices);
        }
    }
}