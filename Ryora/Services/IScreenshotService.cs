using Ryora.Client.Models;
using System.Drawing;

namespace Ryora.Client.Services
{
    public interface IScreenshotService
    {
        ScreenUpdate GetUpdate();
        void ForceUpdate(Rectangle updateRectangle);
    }
}
