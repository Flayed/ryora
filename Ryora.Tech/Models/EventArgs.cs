using System;
using System.Drawing;

namespace Ryora.Tech.Models
{
    public class NewImageEventArgs : EventArgs
    {
        public Rectangle Location { get; set; }
        public byte[] Image { get; set; }

        public NewImageEventArgs(Rectangle location, byte[] image)
        {
            Location = location;
            Image = image;
        }
    }

    public class MouseMoveEventArgs : EventArgs
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }

        public MouseMoveEventArgs(int x, int y, int screenWidth, int screenHeight)
        {
            X = x;
            Y = y;
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;
        }
    }

    public class SharingEventArgs : EventArgs
    {
        public bool IsSharing { get; set; }

        public SharingEventArgs(bool isSharing)
        {
            IsSharing = isSharing;
        }
    }
}
