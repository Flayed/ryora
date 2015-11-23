using System;

namespace Ryora.Client.Models
{
    public class MouseMessageEventArgs : EventArgs
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int WheelDelta { get; set; }
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }
        public bool LeftButton { get; set; }
        public bool MiddleButton { get; set; }
        public bool RightButton { get; set; }
        public bool FirstExtendedButton { get; set; }
        public bool SecondExtendedButton { get; set; }

        public MouseMessageEventArgs(int x, int y, int wheelDelta, int screenWidth, int screenHeight, bool leftButton, bool middleButton, bool rightButton, bool firstExtendedButton, bool secondExtendedButton)
        {
            X = x;
            Y = y;
            WheelDelta = wheelDelta;
            LeftButton = leftButton;
            MiddleButton = middleButton;
            RightButton = rightButton;
            FirstExtendedButton = firstExtendedButton;
            SecondExtendedButton = secondExtendedButton;
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;
        }
    }

    public class KeyboardEventArgs : EventArgs
    {
        public bool IsDown { get; set; }
        public short[] Keys { get; set; }

        public KeyboardEventArgs(bool isDown, short[] keys)
        {
            IsDown = isDown;
            Keys = keys;
        }
    }
}
