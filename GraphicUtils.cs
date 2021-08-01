using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace piskworks
{
    public class GraphicUtils
    {
        public static (List<VertexPositionColor> vertices, List<int> indexes) MakeStraightLine(Vector3 p1, Vector3 p2, 
            float thickness, Color color, int startIdx = 0)
        {
            var vertices = new List<VertexPositionColor>();
            var indexes = new List<int>();

            Vector3 diffHor;
            Vector3 diffVer;
            if (EqualFloats(p1.X, p2.X) && EqualFloats(p1.Y, p2.Y)) {
                diffHor = new Vector3(thickness / 2, 0, 0);
                diffVer = new Vector3(0, thickness / 2, 0);
            }
            else if (EqualFloats(p1.Z, p2.Z) && EqualFloats(p1.Y, p2.Y)) {
                diffHor = new Vector3(0, 0, thickness / 2);
                diffVer = new Vector3(0, thickness / 2, 0);
            }
            else if (EqualFloats(p1.X, p2.X) && EqualFloats(p1.Z, p2.Z)) {
                diffHor = new Vector3(thickness / 2, 0, 0);
                diffVer = new Vector3(0, 0, thickness / 2);
            }
            else {
                return (null, null); // not a horizontal or vertical line
            }
            // horizontal rectangle
            var a = new VertexPositionColor(p1 + diffHor, color);
            var b = new VertexPositionColor(p1 - diffHor, color);
            var c = new VertexPositionColor(p2 + diffHor, color);
            var d = new VertexPositionColor(p2 - diffHor, color);
            vertices.AddRange(new []{a, b, c, d});
            // triangles abc and bcd - indexes start from given start idx
            indexes.AddRange(new [] {startIdx, startIdx + 1, startIdx + 2, startIdx + 1, startIdx + 2, startIdx + 3});
            
            // vertical rectangle
            a = new VertexPositionColor(p1 + diffVer, color);
            b = new VertexPositionColor(p1 - diffVer, color);
            c = new VertexPositionColor(p2 + diffVer,color);
            d = new VertexPositionColor(p2 - diffVer, color);
            vertices.AddRange(new []{a, b, c, d});
            // plus 4 bc there already 4 vertexes added before
            indexes.AddRange(new [] {startIdx + 4, startIdx + 5, startIdx + 6, startIdx + 5, startIdx + 6, startIdx + 7});
            return (vertices, indexes);
        }

        public static bool EqualFloats(float a, float b)
        {
            return Math.Abs(a - b) < 0.0001;
        }
    }
}