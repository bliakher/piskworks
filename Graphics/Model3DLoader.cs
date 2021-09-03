using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace piskworks.Graphics
{
    /// <summary>
    /// 3D model of object.
    /// Consist of vertex and index buffers.
    /// </summary>
    public class Model3D
    {
        /// <summary>
        /// Buffer of vertices
        /// </summary>
        public VertexBuffer VertexBuffer { get; private set; }
        /// <summary>
        /// Buffer of indices into the vertex buffer.
        /// Each three indices make a triangle.
        /// </summary>
        public IndexBuffer IndexBuffer { get; private set; }

        /// <summary>
        /// Creates a model with colored vertices.
        /// </summary>
        public Model3D(GraphicsDevice graphicsDevice, IList<VertexPositionColor> vertices, IList<int> indices)
        {
            VertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), vertices.Count,
                BufferUsage.WriteOnly);
            VertexBuffer.SetData(vertices.ToArray());
            IndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.WriteOnly);
            IndexBuffer.SetData(indices.ToArray());
        }
        
        /// <summary>
        /// Creates a model without information about color.
        /// </summary>
        public Model3D(GraphicsDevice graphicsDevice, IList<VertexPosition> vertices, IList<int> indices)
        {
            VertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPosition), vertices.Count,
                BufferUsage.WriteOnly);
            VertexBuffer.SetData(vertices.ToArray());
            IndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.WriteOnly);
            IndexBuffer.SetData(indices.ToArray());
        }
    }
    
    /// <summary>
    /// Loader of .obj files as <see cref="Model3D"/> objects.
    /// </summary>
    public class Model3DLoader
    {
        private GraphicsDevice _graphicsDevice;

        public Model3DLoader(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        /// <summary>
        /// Load a <see cref="Model3D"/> from the specified file.
        /// </summary>
        /// <param name="sourceFileName">File name of the source .obj file</param>
        /// <returns></returns>
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