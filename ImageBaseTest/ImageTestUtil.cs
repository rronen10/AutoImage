using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Imaging;
using Image = System.Drawing.Image;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace ImageBaseTest
{
    public static class ImageTestUtil
    {
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public static void Click(string imagePath)
        {
            var position = GetImagePosition(imagePath);
            if (position.HasValue)
            {
                var vector = new Size(2, 2);
                Cursor.Position = Point.Add(position.Value, vector);
                mouse_event(MOUSEEVENTF_LEFTDOWN, position.Value.X, position.Value.Y, 0, 0); //make left button down
                System.Threading.Thread.Sleep(200);
                mouse_event(MOUSEEVENTF_LEFTUP, position.Value.X, position.Value.Y, 0, 0); //make left button up
            }
        }

        public static bool Exists(string imagePath)
        {
            return GetImagePosition(imagePath) != null;
        }

        public static Point? GetImagePosition(string imagePath)
        {
            var image = Image.FromFile(imagePath) as Bitmap;
            var screen = GetScreenImage();
            return GetImagePosition(screen, image);
        }

        private static Point? GetImagePosition(Bitmap sourceImage, Bitmap imageTofind)
        {
            BitmapData data = null;
            try
            {
                var tm = new ExhaustiveTemplateMatching(0.921f);

                var matchings = tm.ProcessImage(sourceImage, imageTofind);

                data = sourceImage.LockBits(
                    new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
                    ImageLockMode.ReadWrite, sourceImage.PixelFormat);
                if (matchings.Any())
                    return matchings.First().Rectangle.Location;
                else
                    return null;
            }
            finally
            {
                if (data != null) sourceImage.UnlockBits(data);
            }
        }

        private static Bitmap GetScreenImage()
        {
            var image = new Bitmap(Screen.PrimaryScreen.Bounds.Width, 
                Screen.PrimaryScreen.Bounds.Height, 
                PixelFormat.Format24bppRgb);
            var gfx = Graphics.FromImage(image);
            gfx.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, 
                Screen.PrimaryScreen.Bounds.Y, 0, 0, 
                Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
            return image;
        }
    }
}
