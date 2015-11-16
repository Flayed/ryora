using System.Drawing;

namespace Ryora.Client.Models
{
    public class ScreenUpdate
    {
        public Rectangle Location { get; set; }
        public Bitmap Bitmap { get; set; }

        public ScreenUpdate(Rectangle location, Bitmap bitmap)
        {
            Location = location;
            Bitmap = bitmap;
        }

        public ScreenUpdate(int x, int y, int width, int height, Bitmap bitmap)
            : this(new Rectangle(x, y, width, height), bitmap)
        {
        }
    }
}
