using Ryora.Client.Models;
using System.Collections.Generic;

namespace Ryora.Client.Services
{
    public interface IScreenshotService
    {
        int ScreenWidth { get; }
        int ScreenHeight { get; }
        ScreenUpdate GetUpdate(int x = 0, int y = 0, int? updateWidth = null, int? updateHeight = null);
        IEnumerable<ScreenUpdate> GetUpdates();
    }
}
