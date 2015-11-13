using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Ryora.Client.Services.Implementation;

namespace Ryora.Client.Services
{
    public interface IScreenshotService
    {
        MemoryStream GetScreenshot(Visual target);
        Task<IEnumerable<CachedBitmap>> GetScreenshots();
        void ForceUpdate(Rectangle updateRectangle);
    }
}
