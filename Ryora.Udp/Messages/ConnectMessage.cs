namespace Ryora.Udp.Messages
{
    public class ConnectMessage : IMessage
    {
        public MessageType MessageType { get; } = MessageType.Connect;

        public short ScreenWidth { get; set; }
        public short ScreenHeight { get; set; }

        public ConnectMessage(int screenWidth, int screenHeight)
        {
            ScreenWidth = (short)screenWidth;
            ScreenHeight = (short)screenHeight;
        }

        public ConnectMessage(byte[] payload)
        {
            var offset = 0;
            ScreenWidth = MessageConverter.ReadShort(payload, ref offset);
            ScreenHeight = MessageConverter.ReadShort(payload, ref offset);
        }

        public byte[] ToPayload()
        {
            return MessageConverter.Payloader(ScreenWidth, ScreenHeight);
        }
    }
}
