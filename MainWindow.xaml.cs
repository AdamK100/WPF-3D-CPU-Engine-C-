using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.Runtime.InteropServices;
namespace Direct3D_Test{
public partial class MainWindow : Window
    {
        bool ismeshavailable;
        public int fps;
        public int counter;
        public List<Vector2> vertPx = new List<Vector2>();
        public List<Vector2> edgepx = new List<Vector2>();
        public Mesh mesh;
        public List<Mesh> meshes = new List<Mesh>();
        public DirectBitmap render;
    //Some Code to Convert System.Drawing.Bitmap to ImageSource to then assign it to a WPF image element:
    [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DeleteObject([In] IntPtr hObject);
    public ImageSource ImageSourceFromBitmap(System.Drawing.Bitmap bmp)
    {
        var handle = bmp.GetHbitmap();
        try
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
        }
        finally
        {
            DeleteObject(handle);
        }
    }
    //

        private Timer timer1;
        private Timer timer2;
        //The two following methods are only for counting frames-per-second:
        public void FPSTimer()
        {
            timer2 = new Timer();
            timer2.Tick += new EventHandler(FPS);
            timer2.Interval = (int)1000;
            timer2.Start();
        }
        public void FPS(object sender, EventArgs e)
        {
            Lbl.Content = counter.ToString();
            counter = 0;
        }

        //Frame System
        public void InitTimer()
        {
            timer1 = new Timer();
            timer1.Tick += new EventHandler(Update);
            /* Repeat every 1ms because the FPS amount is not capped, to cap it just replace
               "1" with 1000 divided by the frame cap (like, if i wanted to cap it at 60 fps, i would
               put timer1.Interval = (int)(1000/60);   */
            timer1.Interval = 1;
            timer1.Start();
        }
        //Update is executed every frame
        public void Update(object sender, EventArgs e)
        {
            if (ismeshavailable == true)
            {
                vertPx.Clear();
                edgepx.Clear();
                foreach (Mesh mesh in meshes)
                {
                    /*mesh.Rotation = new Vector3(mesh.Rotation.x, mesh.Rotation.y + (float)30 / 60, mesh.Rotation.z);
                    if (mesh.Rotation.y >= 360)
                    {
                        mesh.Rotation = new Vector3(mesh.Rotation.x, 0, mesh.Rotation.z);
                    } */
                    foreach (Vector3 vertex in mesh.Vertices)
                    {
                        //Model To World To Camera/Screen Calculations
                        Vector3 newPos;
                        newPos = new Vector3(vertex.x * mesh.Scale.x, vertex.y * mesh.Scale.y, vertex.z * mesh.Scale.z);
                        newPos = Toolbox.MatrixToVector3(Toolbox.MultiplyMatrices(Toolbox.MultiplyMatrices(Toolbox.MultiplyMatrices(Toolbox.RotX(mesh), Toolbox.RotY(mesh)), Toolbox.RotZ(mesh)), Toolbox.Vector3ToMatrix(newPos)));
                        newPos = new Vector3(newPos.x + mesh.Position.x, newPos.y + mesh.Position.y, newPos.z + mesh.Position.z);
                        Vector2 pixelposition = new Vector2(newPos.x * (float)rendertex.Width / 16, (float)rendertex.Height - newPos.y * (float)rendertex.Height / 9);
                        edgepx.Add(pixelposition);
                        if (pixelposition.x >= 1 && pixelposition.x <= rendertex.Width && pixelposition.y >= 1 && pixelposition.y <= rendertex.Height)
                        {
                            vertPx.Add(pixelposition);
                        }
                    }

                }
                for (int i = 1; i < render.Width; i++)
                {
                    for (int j = 1; j < render.Height; j++)
                    {
                        render.SetPixel(i, j, System.Drawing.Color.Black);
                    }
                }

                foreach (Vector2 pixel in Toolbox.EdgePixels(Toolbox.ObjEdgeImporter(mesh.Filepath), edgepx.ToArray()))
                {
                    render.SetPixel((int)pixel.x, (int)pixel.y, System.Drawing.Color.Green);
                }
                foreach (Vector2 px in vertPx)
                {
                    render.SetPixel((int)px.x, (int)px.y, System.Drawing.Color.Yellow);
                }
                rendertex.Source = ImageSourceFromBitmap(render.Bitmap);
            }
            counter += 1;
        }
        public MainWindow()
        {
            InitializeComponent();
            render = new DirectBitmap((int)rendertex.Width, (int)rendertex.Height);
            for (int i = 1; i < render.Width; i++)
            {
                for (int j = 1; j < render.Height; j++)
                {
                    render.SetPixel(i, j, System.Drawing.Color.Black);
                }
            }
            rendertex.Source = ImageSourceFromBitmap(render.Bitmap);
            Lbl.Foreground = System.Windows.Media.Brushes.Yellow;
            InitTimer();
            FPSTimer();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog selectsource = new OpenFileDialog();
            selectsource.ShowDialog();
            selectsource.FilterIndex = 1;
            selectsource.Title = "Select a Wavefront .OBJ file";
            string filepath = selectsource.FileName;
            if (filepath.EndsWith(".obj"))
            {
                mesh = new Mesh("test_mesh", Toolbox.ObjVertexImporter(filepath), new Vector3(8, 4.5f, 0), new Vector3(0, 0, 0), new Vector3(1,1,1));
                mesh.Filepath = filepath;
                ismeshavailable = true;
                meshes.Clear();
                meshes.Add(mesh);
                LocX.Text = mesh.Position.x.ToString();
                LocY.Text = mesh.Position.y.ToString();
                LocZ.Text = mesh.Position.z.ToString();
                RotX.Text = mesh.Rotation.x.ToString();
                RotY.Text = mesh.Rotation.y.ToString();
                RotZ.Text = mesh.Rotation.z.ToString();
                ScaleX.Text = mesh.Scale.x.ToString();
                ScaleY.Text = mesh.Scale.y.ToString();
                ScaleZ.Text = mesh.Scale.z.ToString();
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ismeshavailable)
            {
                if (LocX.Text == String.Empty || LocX.Text.Any(x => char.IsLetter(x)))
                {
                    mesh.Position.x = 0;
                }
                else
                {
                    mesh.Position.x = float.Parse(LocX.Text);
                }
            }
        }

        private void LocY_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ismeshavailable)
            {
                if (LocY.Text == String.Empty || LocY.Text.Any(x => char.IsLetter(x)))
                {
                    mesh.Position.y = 0;
                }
                else
                {
                    mesh.Position.y = float.Parse(LocY.Text);
                }
            }
        }

        private void LocZ_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ismeshavailable)
            {
                if (LocZ.Text == String.Empty || LocZ.Text.Any(x => char.IsLetter(x)))
                {
                    mesh.Position.z = 0;
                }
                else
                {
                    mesh.Position.z = float.Parse(LocZ.Text);
                }
            }
        }

        private void RotX_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ismeshavailable)
            {
                if(RotX.Text == String.Empty || RotX.Text.Any(x => char.IsLetter(x)))
                {
                    mesh.Rotation.x = 0;
                }
                else
                {
                    mesh.Rotation.x = float.Parse(RotX.Text);
                }
            }
        }

        private void RotY_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ismeshavailable)
            {
                if (RotY.Text == String.Empty || RotY.Text.Any(x => char.IsLetter(x)))
                {
                    mesh.Rotation.y = 0;
                }
                else
                {
                    mesh.Rotation.y = float.Parse(RotY.Text);
                }
            }   
        }

        private void RotZ_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ismeshavailable)
            {
                if (RotZ.Text == String.Empty || RotZ.Text.Any(x => char.IsLetter(x)))
                {
                    mesh.Rotation.z = 0;
                }
                else
                {
                    mesh.Rotation.z = float.Parse(RotZ.Text);
                }
            }
        }

        private void ScaleX_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ismeshavailable)
            {
                if (ScaleX.Text == String.Empty || ScaleX.Text.Any(x => char.IsLetter(x)))
                {
                    mesh.Scale.x = 0;
                }
                else
                {
                    mesh.Scale.x = float.Parse(ScaleX.Text);
                }
            }
        }

        private void ScaleY_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ismeshavailable)
            {
                if (ScaleY.Text == String.Empty || ScaleY.Text.Any(x => char.IsLetter(x)))
                {
                    mesh.Scale.y = 0;
                }
                else
                {
                    mesh.Scale.y = float.Parse(ScaleY.Text);
                }
            }
        }

        private void ScaleZ_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ismeshavailable)
            {
                if (ScaleZ.Text == String.Empty || ScaleZ.Text.Any(x => char.IsLetter(x)))
                {
                    mesh.Scale.z = 0;
                }
                else
                {
                    mesh.Scale.z = float.Parse(ScaleZ.Text);
                }
            }
        }
    }
}
