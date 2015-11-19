namespace Ryora.Client.Services
{
    public interface IMouseService
    {
        void SetMousePosition(int x, int y, bool leftButton, bool middleButton, bool rightButton);
    }
}
