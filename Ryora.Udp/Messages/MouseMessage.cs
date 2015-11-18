namespace Ryora.Udp.Messages
{
    public class MouseMessage : IMessage
    {
        public MessageType MessageType { get; } = MessageType.MouseMessage;

        public short X { get; set; }
        public short Y { get; set; }
        public ushort ScreenWidth { get; set; }
        public ushort ScreenHeight { get; set; }

        public MouseMessage(int x, int y, int screenWidth, int screenHeight)
        {
            X = (short)x;
            Y = (short)y;
            ScreenWidth = (ushort)screenWidth;
            ScreenHeight = (ushort)screenHeight;
        }

        public MouseMessage(byte[] payload)
        {
            var offset = 0;
            X = MessageConverter.ReadShort(payload, ref offset);
            Y = MessageConverter.ReadShort(payload, ref offset);
            ScreenWidth = MessageConverter.ReadUShort(payload, ref offset);
            ScreenHeight = MessageConverter.ReadUShort(payload, ref offset);
        }

        public byte[] ToPayload()
        {
            return MessageConverter.Payloader(X, Y, ScreenWidth, ScreenHeight);
        }
    }
}
