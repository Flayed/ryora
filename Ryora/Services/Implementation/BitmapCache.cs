using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Ryora.Client.Services.Implementation
{
    public class BitmapCache
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int memcmp(IntPtr b1, IntPtr b2, long count);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static unsafe extern int memcpy(byte* dest, byte* src, long count);

        private const int TimeToLive = 5;

        public int Horizontal { get; set; }
        public int Vertical { get; set; }

        public Size BitmapSize { get; set; }

        private List<CachedBitmap> Cache { get; set; } = new List<CachedBitmap>();

        public BitmapCache(int horizontalBitmaps, int verticalBitmaps, int screenWidth, int screenHeight)
        {
            Horizontal = horizontalBitmaps;
            Vertical = verticalBitmaps;
            BitmapSize = new Size(screenWidth / horizontalBitmaps, screenHeight / verticalBitmaps);
            for (var x = 0; x < screenWidth; x += BitmapSize.Width)
            {
                for (var y = 0; y < screenHeight; y += BitmapSize.Height)
                {
                    Cache.Add(new CachedBitmap(x, y, BitmapSize.Width, BitmapSize.Height));
                }
            }
        }

        public async Task<IEnumerable<CachedBitmap>> CheckBitmaps(Func<Rectangle, Bitmap> getBitmapFunc)
        {
            ConcurrentBag<CachedBitmap> dirtyBitmaps = new ConcurrentBag<CachedBitmap>();
            using (var screenShot = getBitmapFunc(new Rectangle(0, 0, 1920, 1080)))
            {
                //List<CachedBitmap> dirtyBitmaps = new List<CachedBitmap>();
                Cache.ForEach((cachedBitmap) =>
                {
                    try
                    {
                        var target = CropBitmap(screenShot, cachedBitmap.Bounds);
                        if (CompareBitmaps(cachedBitmap.Bitmap, target))
                        {
                            if (--cachedBitmap.TimeToLive > 0)
                                return;
                        }
                        cachedBitmap.Bitmap = target;
                        cachedBitmap.TimeToLive = TimeToLive;
                        dirtyBitmaps.Add(cachedBitmap);
                    }
                    catch
                    {
                        Console.WriteLine("Something bad happend");
                    }
                });
            }
            return dirtyBitmaps;
        }

        public void ForceUpdate(Rectangle rectangle)
        {
            var cachedBitmap = Cache.FirstOrDefault(b => b.Bounds.Equals(rectangle));
            if (cachedBitmap == null) return;
            cachedBitmap.TimeToLive = 0;
        }

        private Bitmap CropBitmap(Bitmap sourceImage, Rectangle rectangle)
        {
            const int BPP = 4; //4 Bpp = 32 bits; argb
            var sourceBitmapdata = sourceImage.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var croppedImage = new Bitmap(rectangle.Width, rectangle.Height, PixelFormat.Format32bppArgb);
            var croppedBitmapData = croppedImage.LockBits(new Rectangle(0, 0, rectangle.Width, rectangle.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            unsafe
            {
                croppedBitmapData.Stride = sourceBitmapdata.Stride;
                byte* sourceImagePointer = (byte*)sourceBitmapdata.Scan0.ToPointer();
                byte* croppedImagePointer = (byte*)croppedBitmapData.Scan0.ToPointer();
                memcpy(croppedImagePointer, sourceImagePointer,
                       Math.Abs(croppedBitmapData.Stride) * rectangle.Height);
            }
            sourceImage.UnlockBits(sourceBitmapdata);
            croppedImage.UnlockBits(croppedBitmapData);
            return croppedImage;
        }

        unsafe private bool CompareBitmaps(Bitmap left, Bitmap right)
        {
            if (left == null || right == null) return false;
            //if (Equals(left, right)) return true;

            bool result = true;
            try
            {
                BitmapData lbmd = left.LockBits(new Rectangle(0, 0, left.Width - 1, left.Height - 1),
                    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                BitmapData rbmd = right.LockBits(new Rectangle(0, 0, right.Width - 1, right.Height - 1),
                    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                try
                {
                    var length = lbmd.Stride * lbmd.Height;
                    result = memcmp(lbmd.Scan0, rbmd.Scan0, length) == 0;
                }
                finally
                {
                    left.UnlockBits(lbmd);
                    right.UnlockBits(rbmd);
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }
    }

    public class CachedBitmap
    {
        public Bitmap Bitmap { get; set; }
        public Rectangle Bounds { get; set; }
        public int TimeToLive { get; set; }

        public CachedBitmap(int x, int y, int width, int height)
        {
            Bounds = new Rectangle(x, y, width, height);
            TimeToLive = 5;
        }
    }
}
