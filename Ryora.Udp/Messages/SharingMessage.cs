namespace Ryora.Udp.Messages
{
    public class SharingMessage : IMessage
    {
        public bool IsSharing { get; set; }
        public MessageType MessageType { get; } = MessageType.Sharing;

        public SharingMessage(bool isSharing)
        {
            IsSharing = isSharing;
        }

        public SharingMessage(byte[] payload)
        {
            var offset = 0;
            IsSharing = MessageConverter.ReadBool(payload, ref offset);
        }

        public byte[] ToPayload()
        {
            return MessageConverter.Payloader(IsSharing);
        }
    }
}
