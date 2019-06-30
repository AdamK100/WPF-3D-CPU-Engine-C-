using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Direct3D_Test
{
    public class Vector3
    {
        public float x;
        public float y;
        public float z;
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public static string String( Vector3 a)
        {
            return ( "(" + a.x.ToString() + "," + a.y.ToString() + "," + a.z.ToString() + ")" );
        }
        public static float Distance(Vector3 a, Vector3 b)
        {
            return ((float)Math.Sqrt((b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y)
                + (b.z - a.z) * (b.z - a.z)));
        }
    }
    public class Vector2
    {
        public float x;
        public float y;
        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
