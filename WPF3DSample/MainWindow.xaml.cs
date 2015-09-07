using _3DTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF3DSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool mouseDown;
        private Point centerOfViewport;
        private double yaw;
        private double pitch;

        public MainWindow()
        {
            CanMoveCamera = true;
            InitializeComponent();
            camera.Transform = new Transform3DGroup();
        }


        public bool CanMoveCamera { get; set; }

        public void Reset()
        {
            camera.Position = new Point3D(camera.Position.X, camera.Position.Y, 5);
            camera.Transform = new Transform3DGroup();
            yaw = 0;
            pitch = 0;
        }


        public void Zoom(double amount)
        {
            //Console.WriteLine("Camera before zoom. X: {0}, Y: {1}", camera.Position.X, camera.Position.Y);
            // For zooming we simply change the Z-position of the camera
            //Point relativePos = Mouse.GetPosition(viewport);
            //Point actualRelativePos = new Point(relativePos.X - viewport.ActualWidth / 2,
            //                                    viewport.ActualHeight / 2 - relativePos.Y);

            //Fix X, Y
            camera.Position = new Point3D(camera.Position.X, camera.Position.Y, camera.Position.Z - amount);

            //Relative to View Port
            //camera.Position = new Point3D(camera.Position.X + actualRelativePos.X, camera.Position.Y + actualRelativePos.Y, camera.Position.Z - amount);
            //Console.WriteLine("Camera after zoom. X: {0}, Y: {1}", camera.Position.X, camera.Position.Y);
        }

        public void Pitch(double amount)
        {
            pitch += amount;
            this.Rotate();
        }

        public void Yaw(double amount)
        {
            yaw += amount;
            this.Rotate();
        }

        private void Rotate()
        {
            double theta = yaw / 3;
            double phi = pitch / 3;

            // Clamp phi (pitch) between -90 and 90 to avoid 'going upside down'
            // Just remove this if you want to make loopings :)
            if (phi < -90) phi = -90;
            if (phi > 90) phi = 90;

            // Here the rotation magic happens. Ask jemidiah for details, I've no clue :P
            Vector3D thetaAxis = new Vector3D(0, 1, 0);
            Vector3D phiAxis = new Vector3D(-1, 0, 0);

            Transform3DGroup transformGroup = camera.Transform as Transform3DGroup;
            transformGroup.Children.Clear();
            //QuaternionRotation3D r0 = (QuaternionRotation3D)((RotateTransform3D)transformGroup.Children[0]).Rotation;
            //r0.Quaternion.Axis.
            QuaternionRotation3D r = new QuaternionRotation3D(new Quaternion(-phiAxis, phi));
            transformGroup.Children.Add(new RotateTransform3D(r));
            r = new QuaternionRotation3D(new Quaternion(-thetaAxis, theta));
            transformGroup.Children.Add(new RotateTransform3D(r));
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.CanMoveCamera)
            {
                if (e.RightButton != MouseButtonState.Pressed) return;

                // Indicate that the right-mouse button is down
                mouseDown = true;

                // Calculate the center of the viewport in screen coordinates
                centerOfViewport = viewport.PointToScreen(new Point(viewport.ActualWidth / 2, viewport.ActualHeight / 2));

                // Set the mouse cursor to that position
                MouseUtilities.SetPosition(centerOfViewport);

                // Hide the cursor
                this.Cursor = Cursors.None;
            }
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Indicate that the mouse is no longer pressed and make the cursor visible again
            mouseDown = false;
            this.Cursor = Cursors.Arrow;
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.CanMoveCamera)
            {
                // If the right-mouse is not pressed, don't do anything
                if (!mouseDown) return;

                // Get mouse position relative to viewport and transform it to the center
                // Literally, actualRelativePos contains the X and Y amounts that the mouse is away from the center of the viewport
                Point relativePos = Mouse.GetPosition(viewport);
                Point actualRelativePos = new Point(relativePos.X - viewport.ActualWidth / 2,
                                                    viewport.ActualHeight / 2 - relativePos.Y);

                // dx and dy are the amounts  by which the mouse moved this move event. Since we keep resetting the mouse to the
                // center, this is just the new position of the mouse, relative to the center: actualRelativePos.
                double dx = actualRelativePos.X;
                double dy = actualRelativePos.Y;

                yaw += dx / 2;
                pitch += dy / 2;

                // Rotate
                this.Rotate();

                // Set mouse position back to the center of the viewport in screen coordinates
                MouseUtilities.SetPosition(centerOfViewport);
            }
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (this.CanMoveCamera)
            {
                // Zoom. Change 100 to a higher value for slower zooming, and vice versa
                this.Zoom(e.Delta / 100);
            }
        }
    }
}
