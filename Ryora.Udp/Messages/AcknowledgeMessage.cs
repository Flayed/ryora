namespace Ryora.Udp.Messages
{
    public class AcknowledgeMessage : IMessage
    {
        public MessageType MessageType { get; } = MessageType.Acknowledge;
        public short ScreenWidth { get; set; }
        public short ScreenHeight { get; set; }

        public AcknowledgeMessage(int screenWidth, int screenHeight)
        {
            ScreenWidth = (short)screenWidth;
            ScreenHeight = (short)screenHeight;
        }

        public AcknowledgeMessage(byte[] payload)
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
