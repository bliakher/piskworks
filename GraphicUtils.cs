using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace piskworks
{
    public class GraphicUtils
    {
        public static (List<Vector3> vertices, List<int> indexes) MakeStraightLine(Vector3 p1, Vector3 p2, float thickness,
            int startIdx = 0)
        {
            var vertices = new List<Vector3>();
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
            var a = p1 + diffHor;
            var b = p1 - diffHor;
            var c = p2 + diffHor;
            var d = p2 - diffHor;
            vertices.AddRange(new []{a, b, c, d});
            // triangles abc and bcd - indexes start from given start idx
            indexes.AddRange(new [] {startIdx, startIdx + 1, startIdx + 2, startIdx + 1, startIdx + 2, startIdx + 3});
            
            // vertical rectangle
            a = p1 + diffVer;
            b = p1 - diffVer;
            c = p2 + diffVer;
            d = p2 - diffVer;
            vertices.AddRange(new []{a, b, c, d});
            indexes.AddRange(new [] {startIdx, startIdx + 1, startIdx + 2, startIdx + 1, startIdx + 2, startIdx + 3});
            return (vertices, indexes);
        }

        public static bool EqualFloats(float a, float b)
        {
            return Math.Abs(a - b) < 0.0001;
        }
    }
}