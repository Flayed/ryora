using System.Runtime.InteropServices;

namespace Ryora.Client.Services.Implementation
{
    public class PinvokeMouseService : IMouseService
    {
        [DllImport("User32.Dll")]
        static extern long SetCursorPos(int x, int y);

        public void SetMousePosition(double x, double y)
        {
            SetCursorPos((int)x, (int)y);
        }
    }
}
