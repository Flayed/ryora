using System;
using System.Drawing;

namespace Ryora.Udp.Messages
{
    public class ImageMessage : IMessage
    {
        public MessageType MessageType { get; } = MessageType.Image;

        public short X { get; set; }
        public short Y { get; set; }
        public ushort Width { get; set; }
        public ushort Height { get; set; }
        public short Sequence { get; set; }

        public Rectangle Location => new Rectangle(X, Y, Width, Height);

        public int ImageLength { get; set; }
        public byte[] ImageData { get; set; }


        public ImageMessage(int x, int y, int width, int height, int imageLength, short sequence, byte[] imageData)
        {
            X = (short)x;
            Y = (short)y;
            Width = (ushort)width;
            Height = (ushort)height;
            ImageLength = imageLength;
            Sequence = sequence;
            ImageData = imageData;
        }

        public ImageMessage(byte[] payload)
        {
            var offset = 0;
            X = MessageConverter.ReadShort(payload, ref offset);
            Y = MessageConverter.ReadShort(payload, ref offset);
            Width = MessageConverter.ReadUShort(payload, ref offset);
            Height = MessageConverter.ReadUShort(payload, ref offset);
            ImageLength = MessageConverter.ReadInt(payload, ref offset);
            Sequence = MessageConverter.ReadShort(payload, ref offset);
            ImageData = new byte[payload.Length - offset];
            Buffer.BlockCopy(payload, offset, ImageData, 0, ImageData.Length);
        }

        public byte[] ToPayload()
        {
            return MessageConverter.Payloader(X, Y, Width, Height, ImageLength, Sequence, ImageData);
        }
    }
}
