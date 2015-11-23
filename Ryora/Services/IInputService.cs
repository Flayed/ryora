namespace Ryora.Client.Services
{
    public interface IInputService
    {
        void SetMousePosition(int x, int y, int sourceWidth, int sourceHeight, bool leftButton, bool middleButton, bool rightButton);
        void SendKeys(bool isDown, short[] keys);
    }
}
