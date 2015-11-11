﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ryora.Tech.Models
{
    public class NewImageEventArgs : EventArgs
    {
        public byte[] Image { get; set; }
        public int Frame { get; set; }

        public NewImageEventArgs(int frame, byte[] image)
        {
            Frame = frame;
            Image = image;
        }
    }

    public class MouseMoveEventArgs : EventArgs
    {
        public double X { get; set; }
        public double Y { get; set; }

        public MouseMoveEventArgs(double x, double y)
        {
            X = x;
            Y = y;
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
