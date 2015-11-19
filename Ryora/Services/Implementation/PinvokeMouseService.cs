using NLog;

namespace Ryora.Client.Services.Implementation
{
    public class PinvokeMouseService : IMouseService
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

        public void SetMousePosition(int x, int y, bool leftButton, bool middleButton, bool rightButton)
        {

        }
    }
}
