namespace Ryora.Client.Services
{
    public interface IInputService
    {
        void SetMousePosition(int x, int y, int wheelDelta, int sourceWidth, int sourceHeight, bool leftButton, bool middleButton, bool rightButton);
        void SendKeys(bool isDown, short[] keys);
        void Reset();
    }
}
