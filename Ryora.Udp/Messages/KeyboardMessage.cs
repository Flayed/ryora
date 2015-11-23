namespace Ryora.Udp.Messages
{
    public class KeyboardMessage : IMessage
    {
        public MessageType MessageType { get; } = MessageType.KeyboardMessage;
        public bool IsDown { get; set; }
        public int NumberOfKeys { get; set; }
        public short[] Keys { get; set; }


        public KeyboardMessage(bool isDown, params short[] keys)
        {
            IsDown = isDown;
            Keys = keys;
            NumberOfKeys = Keys.Length;
        }

        public KeyboardMessage(byte[] payload)
        {
            var offset = 1;
            byte metabyte = payload[0];
            IsDown = (metabyte & 1) == 1;
            NumberOfKeys = metabyte >> 1;
            Keys = new short[NumberOfKeys];
            for (var idx = 0; idx < NumberOfKeys; idx++)
                Keys[idx] = MessageConverter.ReadShort(payload, ref offset);
        }

        public byte[] ToPayload()
        {
            byte metabyte = (byte)((NumberOfKeys << 1) | (IsDown ? 1 : 0));
            return MessageConverter.Payloader(metabyte, Keys);
        }
    }
}
