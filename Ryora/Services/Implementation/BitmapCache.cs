using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media.Imaging;

namespace Ryora.Client.Services.Implementation
{
    public class BitmapCache
    {    
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int memcmp(IntPtr b1, IntPtr b2, long count);

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

        public async Task<IEnumerable<CachedBitmap>>  CheckBitmaps(Func<Rectangle, Bitmap> getBitmapFunc)
        {
            ConcurrentBag<CachedBitmap> dirtyBitmaps = new ConcurrentBag<CachedBitmap>();
            //List<CachedBitmap> dirtyBitmaps = new List<CachedBitmap>();
            Parallel.ForEach(Cache, (cachedBitmap) =>
            {
                try
                {
                    var target = getBitmapFunc(cachedBitmap.Bounds);
                    if (CompareBitmaps(cachedBitmap.Bitmap, target))
                    {
                        if (--cachedBitmap.TimeToLive > 0)                        
                            return;
                    }
                    cachedBitmap.Bitmap = target;
                    cachedBitmap.TimeToLive = 100;
                    dirtyBitmaps.Add(cachedBitmap);                    
                }
                catch
                {
                    Console.WriteLine("Something bad happend");
                }
            });
            return dirtyBitmaps;
        }

        public void ForceUpdate(Rectangle rectangle)
        {
            var cachedBitmap = Cache.FirstOrDefault(b => b.Bounds.Equals(rectangle));
            if (cachedBitmap == null) return;
            cachedBitmap.TimeToLive = 0;
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
            TimeToLive = 100;
        }
    }    
}
