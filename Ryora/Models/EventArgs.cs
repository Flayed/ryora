using System;

namespace Ryora.Client.Models
{
    public class MouseMessageEventArgs : EventArgs
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }

        public MouseMessageEventArgs(int x, int y, int screenWidth, int screenHeight)
        {
            X = x;
            Y = y;
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;
        }
    }
}
