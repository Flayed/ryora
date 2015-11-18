using Ryora.Udp.Messages;
using System;
using System.Text;

namespace Ryora.Udp
{
    public enum MessageType
    {
        Connect = 0,
        Disconnect = 1,
        Acknowledge = 2,
        MouseMessage = 3,
        Image = 4,

        //Data = 3,
        //DataFragment = 4,
        //LastDataFragment = 5
    }

    public static class Messaging
    {
        public static byte[] CreateMessage(IMessage messageInformation, short clientId, short channel, short messageId)
        {
            return MessageConverter.Payloader(
                GenerateHeader(messageInformation.MessageType, clientId, channel, messageId),
                messageInformation.ToPayload());
        }

        public static byte[] CreateMessage(MessageType messageType, short clientId, short channel, short messageId, byte[] payload)
        {
            return MessageConverter.Payloader(
                GenerateHeader(messageType, clientId, channel, messageId),
                payload);
        }

        public static byte[] CreateMessage(MessageType messageType, short clientId, short channel, short messageId, string message = "")
        {
            var payload = Encoding.Default.GetBytes(message);
            return CreateMessage(messageType, clientId, channel, messageId, payload);
        }

        public static byte[] CreateMessage(UdpMessage message)
        {
            return CreateMessage(message.Type, message.ConnectionId, message.Channel, message.MessageId, message.Payload);
        }

        private static byte[] GenerateHeader(MessageType messagetype, short connectionId, short channel, short messageId)
        {
            var header = MessageConverter.Payloader(messagetype, connectionId, channel, messageId);
            return header;
        }

        public static UdpMessage ReceiveMessage(byte[] message)
        {
            UdpMessage msg = new UdpMessage();

            var offset = 1;
            msg.Type = (MessageType)message[0];
            msg.ConnectionId = MessageConverter.ReadShort(message, ref offset);
            msg.Channel = MessageConverter.ReadShort(message, ref offset);
            msg.MessageId = MessageConverter.ReadShort(message, ref offset);

            msg.Payload = new byte[message.Length - offset];
            Buffer.BlockCopy(message, offset, msg.Payload, 0, msg.Payload.Length);

            return msg;
        }
    }

    public class UdpMessage
    {
        public MessageType Type { get; set; }
        public short ConnectionId { get; set; }
        public short Channel { get; set; }
        public short MessageId { get; set; }
        public byte[] Payload { get; set; }

        public string Message => Encoding.Default.GetString(Payload);

        public UdpMessage()
        {
        }

        public UdpMessage(MessageType type, short connectionId, short channel, short messageId, byte[] payload)
        {
            Type = type;
            ConnectionId = connectionId;
            Channel = channel;
            MessageId = messageId;
            Payload = payload;

        }

        public UdpMessage(MessageType type, short connectionId, short channel, short messageId, string message)
            : this(type, connectionId, channel, messageId, Encoding.Default.GetBytes(message))
        {

        }

        public byte[] Bytes => Messaging.CreateMessage(this);
    }
}
