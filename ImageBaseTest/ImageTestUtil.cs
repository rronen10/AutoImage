using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using AForge.Imaging;
using Image = System.Drawing.Image;
using System.Runtime.InteropServices;
using System.Threading;
using AForge.Imaging.Filters;
using System.IO;

namespace ImageBaseTest
{
    public static class ImageTestUtil
    {
        #region Consts
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        private const int MOUSEEVENTF_MOVE = 0x01;

        #endregion

        #region COM Methods
        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        #endregion

        #region Members
        private static Size? _screenSearchSize;
        private static Point? _positionSearch;
        #endregion

        #region Properties
        public static string DebugFolderPath { get; set; }
        #endregion

        #region Public methods
        public static Point? GetImagePosition(string imagePath, int waitTimeoutInSeconds = 30, bool isCancelSearchOptimization = false, float similarityThreshold = 0.92f)
        {
            Size dummy;
            return GetImagePosition(out dummy,
                imagePath,
                waitTimeoutInSeconds,
                isCancelSearchOptimization,
                similarityThreshold);
        }

        public static void Click(string imagePath, bool isDoubleClick = false, Size? imageClickPointOffset = null, bool isRightClick = false, int waitTimeoutInSeconds = 60, bool isCancelSearchOptimization = true, Size? dragPosition = null, float similarityThreshold = 0.92f)
        {
            Size imageCenterPoint;
            var position = GetImagePosition(out imageCenterPoint,
                imagePath,
                waitTimeoutInSeconds,
                isCancelSearchOptimization,
                similarityThreshold);

            if (imageClickPointOffset.HasValue)
                imageCenterPoint = imageClickPointOffset.Value;

            if (position.HasValue)
            {

                Cursor.Position = Point.Add(position.Value, imageCenterPoint);

                mouse_event(isRightClick ? MOUSEEVENTF_RIGHTDOWN : MOUSEEVENTF_LEFTDOWN,
                    position.Value.X,
                    position.Value.Y, 0, 0);
                if (dragPosition.HasValue)
                    MoveDragPosition(position.Value, dragPosition.Value);
                Thread.Sleep(10);
                mouse_event(isRightClick ? MOUSEEVENTF_RIGHTUP : MOUSEEVENTF_LEFTUP,
                    position.Value.X,
                    position.Value.Y, 0, 0);

                if (isDoubleClick)
                {
                    mouse_event(MOUSEEVENTF_LEFTDOWN,
                    position.Value.X,
                    position.Value.Y, 0, 0);
                    Thread.Sleep(10);
                    mouse_event(MOUSEEVENTF_LEFTUP,
                    position.Value.X,
                    position.Value.Y, 0, 0);
                }
            }
            else
                throw new Exception(string.Format("Image was not found:{0}", imagePath));
        }

        private static void MoveDragPosition(Point currentPosision, Size dragPositionOffset)
        {
            Cursor.Position = Point.Add(currentPosision, dragPositionOffset);
        }

        public static bool Exists(string imagePath, int waitTimeoutInSeconds = 30, bool isCancelSearchOptimization = false, float similarityThreshold = 0.92f)
        {
            return GetImagePosition(imagePath,
                waitTimeoutInSeconds,
                isCancelSearchOptimization,
                similarityThreshold
                ).HasValue;
        }

        public static void Type(string value)
        {
            SendKeys.SendWait(value);
        }

        public static void SetScreenSearchSize(Size? screenSize, Point? position)
        {
            _screenSearchSize = screenSize;
            _positionSearch = position;
        }

        public static Size GetScreenSize()
        {
            return Screen.PrimaryScreen.Bounds.Size;
        }
        #endregion

        #region Private Methods
        private static Point? GetImagePosition(out Size imageCenterPoint, string imagePath, int waitTimeoutInSeconds, bool isCancelSearchOptimization, float similarityThreshold)
        {
            var imageTofind = Image.FromFile(imagePath) as Bitmap;
            if (imageTofind == null)
                throw new BadImageFormatException("Image {0} cannot convert to Bitmap");

            imageCenterPoint = new Size(imageTofind.Width / 2, imageTofind.Height / 2);
            var sourceImage = GetScreenImage();

            return GetImagePosition(sourceImage,
                imageTofind,
                waitTimeoutInSeconds,
                isCancelSearchOptimization,
                similarityThreshold);
        }

        private static string HandleDebugMode(Bitmap imageTofind, Bitmap sourceImage, bool isResize = false, string debugFolder = null)
        {
            if (!string.IsNullOrWhiteSpace(DebugFolderPath))
            {
                var folderName = DateTime.Now.ToString().Replace("/", "-").Replace(" ", string.Empty).Replace(":", "_");

                string debugDir;
                if (string.IsNullOrWhiteSpace(debugFolder))
                    debugDir = Directory.CreateDirectory(Path.Combine(DebugFolderPath, folderName)).FullName;
                else
                    debugDir = debugFolder;

                imageTofind.Save(Path.Combine(debugDir, isResize ? "resize_imageTofind.png" : "imageTofind.png"));
                sourceImage.Save(Path.Combine(debugDir, isResize ? "resize_sourceImage.png" : "sourceImage.png"));
                return debugDir;
            }
            return null;
        }

        private static void HandleDebugMode(string debugFolder, Rectangle rectangle, Bitmap sourceImage)
        {
            if (!string.IsNullOrWhiteSpace(DebugFolderPath))
            {
                var data = sourceImage.LockBits(
                    new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
                    ImageLockMode.ReadWrite, sourceImage.PixelFormat);
                Drawing.Rectangle(data, rectangle, Color.Red);

                sourceImage.Save(Path.Combine(debugFolder, string.Format("clickImage_{0}x{1}.png", rectangle.Location.X, rectangle.Location.Y)));
            }

        }

        private static void HandleDebugMode(string debugFolder)
        {
            if (!string.IsNullOrWhiteSpace(DebugFolderPath))
            {
                File.Create(Path.Combine(debugFolder, "ImageNotFound.txt"));
            }
        }

        private static Point? GetImagePosition(Bitmap sourceImage, Bitmap imageTofind, int waitTimeoutInSeconds, bool isCancelSearchOptimization, float similarityThreshold)
        {
            var tm = new ExhaustiveTemplateMatching(similarityThreshold);

            var timeoutDate = DateTime.Now.AddSeconds(waitTimeoutInSeconds);

            var result = GetImagePosition(sourceImage, imageTofind, tm, isCancelSearchOptimization);
            while (!result.HasValue
                && DateTime.Now < timeoutDate)
            {
                sourceImage = GetScreenImage();
                result = GetImagePosition(sourceImage, imageTofind, tm, isCancelSearchOptimization);
            }
            return result;
        }


        private static Point? GetImagePosition(Bitmap sourceImage, Bitmap imageTofind, ExhaustiveTemplateMatching tm, bool isCancelSearchOptimization)
        {
            string debugFolder = null;
            Point? result;
            if (!isCancelSearchOptimization)
            {
                result = SearchOptimazeImage(out debugFolder, sourceImage, imageTofind, tm);
                if (result.HasValue)
                    return result;
            }

            debugFolder = HandleDebugMode(imageTofind, sourceImage, debugFolder: debugFolder);
            var matchings = tm.ProcessImage(
                sourceImage, imageTofind);

            if (matchings.Any())
            {
                var rectangle = matchings.First().Rectangle;
                result = rectangle.Location;

                if (_positionSearch.HasValue)
                {
                    if (_positionSearch.Value.X > 0)
                        result = new Point(result.Value.X + _positionSearch.Value.X, result.Value.Y);
                    if (_positionSearch.Value.Y > 0)
                        result = new Point(result.Value.X, result.Value.Y + _positionSearch.Value.Y);
                }

                HandleDebugMode(debugFolder, rectangle, sourceImage);
            }
            else
            {
                result = null;
                HandleDebugMode(debugFolder);
            }
            return result;

        }

        private static Point? SearchOptimazeImage(out string debugFolder, Bitmap sourceImage, Bitmap imageTofind, ExhaustiveTemplateMatching tm)
        {
            const int divisor = 2;

            var resizeSourceImage = new ResizeNearestNeighbor(sourceImage.Width / divisor, sourceImage.Height / divisor).Apply(sourceImage);
            var resizeImageTofind = new ResizeNearestNeighbor(imageTofind.Width / divisor, imageTofind.Height / divisor).Apply(imageTofind);

            var matchings = tm.ProcessImage(resizeSourceImage, resizeImageTofind);

            debugFolder = HandleDebugMode(resizeImageTofind, resizeSourceImage, true);

            if (matchings.Any())
            {
                var rectangle = matchings.First().Rectangle;
                var result = new Point(rectangle.Location.X * divisor,
                    rectangle.Location.Y * divisor);
                HandleDebugMode(debugFolder, rectangle, resizeSourceImage);
                return result;
            }
            return null;
        }

        private static Bitmap GetScreenImage()
        {
            var image = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                Screen.PrimaryScreen.Bounds.Height,
                PixelFormat.Format24bppRgb);

            var gfx = Graphics.FromImage(image);

            gfx.CopyFromScreen(0, 0
                 , Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y,
                Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);

            CropImage(ref image);

            return image;
        }

        private static void CropImage(ref Bitmap src)
        {
            if (_screenSearchSize.HasValue)
            {
                var screenWidth = Screen.PrimaryScreen.Bounds.Width;
                var screenHeight = Screen.PrimaryScreen.Bounds.Height;
                var screenX = 0;
                var screenY = 0;


                if (_screenSearchSize.Value.Width > 0)
                {
                    screenWidth = _screenSearchSize.Value.Width;
                }
                if (_screenSearchSize.Value.Height > 0)
                {
                    screenHeight = _screenSearchSize.Value.Height;
                }

                if (_positionSearch.HasValue && _positionSearch.Value.X > 0)
                {
                    screenX = _positionSearch.Value.X;
                }
                if (_positionSearch.HasValue && _positionSearch.Value.Y > 0)
                {
                    screenY = _positionSearch.Value.Y;
                }

                var cropRect = new Rectangle(screenX,
                    screenY,
                    Screen.PrimaryScreen.Bounds.Width - screenX,
                    Screen.PrimaryScreen.Bounds.Height - screenY);

                var target = new Bitmap(cropRect.Width, cropRect.Height, PixelFormat.Format24bppRgb);

                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
                        cropRect,
                        GraphicsUnit.Pixel);
                }

                src = target;
            }
        }

        #endregion
    }
}
