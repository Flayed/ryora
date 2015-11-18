using Ryora.Client.Models;
using System.Collections.Generic;
using System.Drawing;

namespace Ryora.Client.Services
{
    public interface IScreenshotService
    {
        int ScreenWidth { get; }
        int ScreenHeight { get; }
        ScreenUpdate GetUpdate();
        IEnumerable<ScreenUpdate> GetUpdates();
        void ForceUpdate(Rectangle updateRectangle);
    }
}
