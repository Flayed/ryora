﻿using Ryora.Tech.Services.Implementation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ryora.Tech.Services
{
    interface IRealtimeService
    {
        event EventHandler NewImage;
        event EventHandler MouseMove;
        event EventHandler Sharing;
        event EventHandler ClientResolutionChanged;

        Task StartConnection(short channel, int screenWidth, int screenHeight);
        Task EndConnection(short channel);

        Task SendMouseCoords(short channel, int x, int y, int screenWidth, int screenHeight, bool leftButton, bool middleButton, bool rightButton, bool firstExtendedButton, bool secondExtendedButton);

        Task SendKeyboardInput(short channel, bool isDown, params short[] virtualKeyCodes);

        string Transport { get; }

        IEnumerable<ImageFragment> CompletedImages { get; }
    }
}
