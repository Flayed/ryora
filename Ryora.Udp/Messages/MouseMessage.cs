namespace Ryora.Udp.Messages
{
    public class MouseMessage : IMessage
    {
        public MessageType MessageType { get; } = MessageType.MouseMessage;

        public short X { get; set; }
        public short Y { get; set; }
        public short WheelDelta { get; set; }
        public bool LeftButton { get; set; }
        public bool MiddleButton { get; set; }
        public bool RightButton { get; set; }
        public bool FirstExtendedButton { get; set; }
        public bool SecondExtendedButton { get; set; }
        public ushort ScreenWidth { get; set; }
        public ushort ScreenHeight { get; set; }

        public MouseMessage(int x, int y, int wheelDelta, int screenWidth, int screenHeight, bool leftButton = false, bool middleButton = false, bool rightButton = false, bool firstExtendedButton = false, bool secondExtendedButton = false)
        {
            X = (short)x;
            Y = (short)y;
            ScreenWidth = (ushort)screenWidth;
            ScreenHeight = (ushort)screenHeight;
            LeftButton = leftButton;
            MiddleButton = middleButton;
            RightButton = rightButton;
            FirstExtendedButton = firstExtendedButton;
            SecondExtendedButton = secondExtendedButton;
            WheelDelta = (short)wheelDelta;
        }

        public MouseMessage(byte[] payload)
        {
            var offset = 0;
            X = MessageConverter.ReadShort(payload, ref offset);
            Y = MessageConverter.ReadShort(payload, ref offset);
            WheelDelta = MessageConverter.ReadShort(payload, ref offset);
            ScreenWidth = MessageConverter.ReadUShort(payload, ref offset);
            ScreenHeight = MessageConverter.ReadUShort(payload, ref offset);
            var boolArray = MessageConverter.ReadBoolArray(payload, ref offset);
            LeftButton = boolArray[0];
            MiddleButton = boolArray[1];
            RightButton = boolArray[2];
            FirstExtendedButton = boolArray[3];
            SecondExtendedButton = boolArray[4];
        }

        public byte[] ToPayload()
        {
            return MessageConverter.Payloader(X, Y, WheelDelta, ScreenWidth, ScreenHeight,
                new bool[] { LeftButton, MiddleButton, RightButton, FirstExtendedButton, SecondExtendedButton });
        }
    }
}
