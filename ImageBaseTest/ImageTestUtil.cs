using System;
using System.CodeDom;
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
        #region Consts
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        #endregion

        #region COM Methods
        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        #endregion

        #region Public methods
        public static void Click(string imagePath, Size? imageClickPointOffset = null,bool isRightClick = false)
        {
            Size imageCenterPoint;
            var position = GetImagePosition(out imageCenterPoint, imagePath);
            if (imageClickPointOffset.HasValue)
                imageCenterPoint = imageClickPointOffset.Value;

            if (position.HasValue)
            {
                Cursor.Position = Point.Add(position.Value, imageCenterPoint);

                mouse_event(isRightClick? MOUSEEVENTF_RIGHTDOWN:MOUSEEVENTF_LEFTDOWN, 
                    position.Value.X, 
                    position.Value.Y, 0, 0); 
                System.Threading.Thread.Sleep(200);
                mouse_event(isRightClick? MOUSEEVENTF_RIGHTUP:MOUSEEVENTF_LEFTUP, 
                    position.Value.X, 
                    position.Value.Y, 0, 0); 
            }
        }

        public static bool Exists(string imagePath)
        {
            Size dummy;
            return GetImagePosition(out dummy,imagePath).HasValue;
        }

        public static void Type(string value)
        {
            SendKeys.Send(value);
        }
        #endregion

        #region Private Methods
        private static Point? GetImagePosition(out Size imageCenterPoint,string imagePath)
        {
            var image = Image.FromFile(imagePath) as Bitmap;
            if(image == null)
                throw new BadImageFormatException("Image {0} cannot convert to Bitmap");

            imageCenterPoint = new Size(image.Width/2, image.Height/2);
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
        #endregion
    }
}
