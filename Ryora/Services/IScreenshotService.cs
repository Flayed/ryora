using Ryora.Client.Models;
using System.Collections.Generic;
using System.Drawing;

namespace Ryora.Client.Services
{
    public interface IScreenshotService
    {
        ScreenUpdate GetUpdate();
        IEnumerable<ScreenUpdate> GetUpdates();
        void ForceUpdate(Rectangle updateRectangle);
    }
}
