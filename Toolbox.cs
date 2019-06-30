using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

namespace Direct3D_Test
{
    //The DirectBitmap class permits faster "SetPixel" and "GetPixel" methods which are originally costly on performance
    public class DirectBitmap : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        public Int32[] Bits { get; private set; }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }
        protected GCHandle BitsHandle { get; private set; }
        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new Int32[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
        }
        public void SetPixel(int x, int y, Color colour)
        {
            int index = x + (y * Width);
            int col = colour.ToArgb();
            if (index >= 0 && index < Bits.Length)
            {
                Bits[index] = col;
            }
        }
        public Color GetPixel(int x, int y)
        {
            int index = x + (y * Width);
            int col = Bits[index];
            Color result = Color.FromArgb(col);
            return result;
        }
        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
        }
    }
    class Toolbox
    {
        public static Vector3 MatrixToVector3(float[,] matrix)
        {
            return new Vector3(matrix[0,0], matrix[1,0], matrix[2,0]);
        }
        public static float[,] Vector3ToMatrix(Vector3 coords)
        {
            return new float[3, 1]
            { {coords.x },
            { coords.y },
            { coords.z }
            };
        }
        public static int i, j;
        //Quick math methods
        public static float DotProduct(Vector3 a, Vector3 b)
        {
            return (a.x* b.x + a.y*b.y + a.z*b.z);
        }
        public static Vector3 CrossProduct(Vector3 a, Vector3 b)
        {
            return new Vector3( (a.y * b.z) - (a.z * b.y) , -((a.x * b.z) - (a.z * b.x)), (a.x * b.y) - (a.y * b.x));
        }
        public static string CharsToString(char[] car)
        {
            string a = string.Empty;
            foreach(char c in car)
            {
                a = a + char.ToString(c);
            }
            return a;
        }

        // Rotation Matrices
        public static float[,] RotX(Mesh mesh)
        {
            return new float[3, 3]
            {
                { 1 , 0 , 0},
                { 0, (float)Math.Cos(mesh.Rotation.x * Math.PI/180), -(float)Math.Sin(mesh.Rotation.x * Math.PI/180)},
                {0, (float)Math.Sin(mesh.Rotation.x * Math.PI/180), (float)Math.Cos(mesh.Rotation.x * Math.PI/180) }
            };
        }
        public static float[,] RotY(Mesh mesh)
        {
            return new float[3, 3]
            {
                { (float)Math.Cos(mesh.Rotation.y * Math.PI/180) , 0 , (float)Math.Sin(mesh.Rotation.y * Math.PI/180)},
                { 0, 1, 0},
                {-(float)Math.Sin(mesh.Rotation.y * Math.PI/180), 0, (float)Math.Cos(mesh.Rotation.y * Math.PI/180) }
            };
        }
        public static float[,] RotZ(Mesh mesh)
        {
            return new float[3, 3]
            {
                { (float)Math.Cos(mesh.Rotation.z * Math.PI/180) , -(float)Math.Sin(mesh.Rotation.z * Math.PI/180) , 0},
                { (float)Math.Sin(mesh.Rotation.z * Math.PI/180), (float)Math.Cos(mesh.Rotation.z * Math.PI/180), 0},
                {0, 0, 1 }
            };
        }

        public static Vector2[] EdgePixels(Edge[] edges, Vector2[] PixelPositions)
        {
            List<Vector2> pxedges = new List<Vector2>();
            foreach (Edge edge in edges)
            {
                Vector2 v1 = PixelPositions[edge.v1 - 1];
                Vector2 v2 = PixelPositions[edge.v2 - 1];
                float slope = (v2.y - v1.y) / (v2.x - v1.x);
                float ord = v2.y - slope*v2.x;
                if(v1.x == v2.x)
                {
                    for (int i = 1; i <= v2.y - v1.y; i++)
                    {
                        pxedges.Add(new Vector2(v1.x,v1.y + i * (v2.y - v1.y) / Math.Abs(v2.y - v1.y)));
                    }
                }
                if(v2.x > v1.x)
                {
                    for (int i = 1; i <= v2.x - v1.x; i++)
                    {
                        pxedges.Add(new Vector2(v1.x + i, (float)Math.Round(slope*(v1.x + i) + ord)));
                    }
                }
                if(v2.x < v1.x)
                {
                    for (int i = 1; i <= v1.x - v2.x; i++)
                    {
                        pxedges.Add(new Vector2(v2.x + i, (float)Math.Round(slope * (v2.x + i) + ord)));
                    }
                }
            }
            return pxedges.ToArray();
        }
        public static Edge[] ObjEdgeImporter(string filepath)
        {
            List<int> faceverts = new List<int>();
            List<Edge> edges = new List<Edge>();
            var lines = File.ReadLines(filepath);
            foreach (var line in lines)
            {
                bool recording = false;
                string Numrec = string.Empty;
                if (line.StartsWith("f "))
                {
                    faceverts.Clear();
                    for (int i = 1; i < line.Length; i++)
                    {
                        if (char.IsDigit(line[i]) && char.IsWhiteSpace(line[i - 1]))
                        {
                            if (recording == false)
                            {
                                Numrec = string.Empty;
                                recording = true;
                            }
                        }
                        if(recording == true && char.IsDigit(line[i]))
                        {
                            Numrec += line[i].ToString();
                        }
                        if (recording == true && !char.IsDigit(line[i]))
                        {
                            recording = false;
                            faceverts.Add(int.Parse(Numrec));
                        }
                    }
                    if (faceverts.ToArray().Length == 4)
                    {
                        edges.Add(new Edge(faceverts[0], faceverts[1]));
                        edges.Add(new Edge(faceverts[0], faceverts[3]));
                        edges.Add(new Edge(faceverts[1], faceverts[2]));
                        edges.Add(new Edge(faceverts[2], faceverts[3]));
                    }
                    if (faceverts.ToArray().Length == 3)
                    {
                        edges.Add(new Edge(faceverts[0],faceverts[1]));
                        edges.Add(new Edge(faceverts[0],faceverts[2]));
                        edges.Add(new Edge(faceverts[1],faceverts[2]));
                    }
                }
            }
            return edges.ToArray();
        }
        public static Vector3[] ObjVertexImporter(string filepath)
        {
            List<float> xverts = new List<float>();
            List<float> yverts = new List<float>();
            List<float> zverts = new List<float>();
            List<Vector3> vertices = new List<Vector3>();
            List<char> numRec = new List<char>();
            bool recordingX;
            bool stopX;
            bool recordingY;
            bool stopY;
            bool recordingZ;
            var lines = File.ReadLines(filepath);
            foreach (var line in lines)
            {
                if (line.StartsWith("v "))
                {
                    recordingX = false;
                    recordingY = false;
                    recordingZ = false;
                    stopX = false;
                    stopY = false;
                    for (int i = 0; i < line.Length; i++)
                    {
                        if (( i > 1 && (char.IsDigit(line[i]) || line[i] == '-') && char.IsWhiteSpace(line[i - 1])))
                        {
                            if (!recordingX && stopX == false && stopY == false)
                            {
                                numRec.Clear();
                                recordingX = true;
                            }
                            if (!recordingY && stopX == true && stopY == false)
                            {
                                numRec.Clear();
                                recordingY = true;
                            }
                            if (!recordingZ && stopX == true && stopY == true)
                            {
                                numRec.Clear();
                                recordingZ = true;
                            }
                        }
                        if (recordingX)
                        {
                            if (char.IsDigit(line[i]) || char.IsPunctuation(line[i]))
                            {
                                numRec.Add(line[i]);
                            }
                            if (char.IsWhiteSpace(line[i]))
                            {
                                xverts.Add(float.Parse(Toolbox.CharsToString(numRec.ToArray())));
                                stopX = true;
                                recordingX = false;
                            }
                        }
                        if (recordingY)
                        {
                            if (char.IsDigit(line[i]) || char.IsPunctuation(line[i]))
                            {
                                numRec.Add(line[i]);
                            }
                            if (char.IsWhiteSpace(line[i]))
                            {
                                yverts.Add(float.Parse(Toolbox.CharsToString(numRec.ToArray())));
                                stopY = true;
                                recordingY = false;
                            }
                        }
                        if (recordingZ)
                        {
                            if (char.IsDigit(line[i]) || char.IsPunctuation(line[i]))
                            {
                                numRec.Add(line[i]);
                            }
                            if ( i == line.Length - 1)
                            {
                                zverts.Add(float.Parse(Toolbox.CharsToString(numRec.ToArray())));
                                stopX = false;
                                recordingZ = false;
                            }
                        }
                    }
                }
            }
            for(int i = 0; i<xverts.ToArray().Length; i++)
            {
                Vector3 a = new Vector3(xverts[i],yverts[i],zverts[i]);
                vertices.Add(a);
            }
            return vertices.ToArray();
        }
        public static float[,] MultiplyMatrices(float[,] a, float[,] b)
        {
                float[,] c = new float[a.GetLength(0), b.GetLength(1)];
                for (i = 0; i < a.GetLength(0); i++)
                {
                    for (j = 0; j < b.GetLength(1); j++)
                    {
                        c[i, j] = 0;
                        for (int k = 0; k < a.GetLength(1); k++)
                        {
                            c[i, j] += a[i, k] * b[k, j];
                        }
                    }
                }
                return c;
        }
    }
}
