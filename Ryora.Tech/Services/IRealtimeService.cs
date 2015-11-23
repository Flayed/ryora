using Ryora.Tech.Services.Implementation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ryora.Tech.Services
{
    interface IRealtimeService
    {
        event EventHandler MouseMove;
        event EventHandler<bool> Sharing;
        event EventHandler ClientResolutionChanged;
        event EventHandler<bool> Disconnect;

        Task StartConnection(short channel, int screenWidth, int screenHeight);
        Task EndConnection(short channel, bool reconnect = false);

        Task SendMouseCoords(short channel, int x, int y, int mouseWheelDelta, int screenWidth, int screenHeight, bool leftButton, bool middleButton, bool rightButton, bool firstExtendedButton, bool secondExtendedButton);

        Task SendKeyboardInput(short channel, bool isDown, params short[] virtualKeyCodes);

        string Transport { get; }

        IEnumerable<ImageFragment> CompletedImages { get; }
    }
}
