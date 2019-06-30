using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using System.Drawing;
namespace Direct3D_Test
{

    public class Camera
    {
        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }
    }
    public class Edge
    {
        public int v1 { get; set; }
        public int v2 { get; set; }
        public Edge(int V1, int V2)
        {
            v1 = V1;
            v2 = V2;
        }
    }

    public class Mesh
    {
        public string Filepath { get; set; }
        public string Name { get; set; }
        public Vector3[] Vertices { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public Vector3 Scale { get; set; }
        public Mesh(string name, Vector3[] vertices, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            Name = name;
            Vertices = vertices;
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }
    }
    class EngineCore
    {
    }
}
