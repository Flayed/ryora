using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ryora.Udp
{
    public enum MessageType
    {
        Connect = 0,
        Disconnect = 1,
        Acknowledge = 2,
        Data = 3,
        DataFragment = 4,
        LastDataFragment = 5
    }

    public static class Messaging
    {
        public static byte[] CreateMessage(MessageType messageType, short clientId, short channel, short messageId, short sequence, byte[] payload)
        {
            var header = GenerateHeader(messageType, clientId, channel, messageId, sequence);
            var data = new byte[header.Length + payload.Length];
            Buffer.BlockCopy(header, 0, data, 0, header.Length);
            Buffer.BlockCopy(payload, 0, data, header.Length, payload.Length);
            return data;
        }

        public static byte[] CreateMessage(MessageType messageType, short clientId, short channel, short messageId, short sequence, string message)
        {
            var payload = Encoding.Default.GetBytes(message);
            return CreateMessage(messageType, clientId, channel, messageId, sequence, payload);
        }

        public static byte[] CreateMessage(UdpMessage message)
        {
            return CreateMessage(message.Type, message.ConnectionId, message.Channel, message.MessageId, message.Sequence, message.Payload);
        }

        private static int HeaderLength { get; set; } = 9;
        private static byte[] GenerateHeader(MessageType messagetype, short connectionId, short channel, short messageId, short sequence)
        {
            var header = new byte[HeaderLength];
            header[0] = (byte)((int)messagetype);
            header[1] = (byte) (connectionId >> 8);
            header[2] = (byte) (connectionId & 255);
            header[3] = (byte) (channel >> 8);
            header[4] = (byte) (channel & 255);
            header[5] = (byte) (messageId >> 8);
            header[6] = (byte) (messageId & 255);
            header[7] = (byte) (sequence >> 8);
            header[8] = (byte) (sequence & 255);
            return header;
        }

        public static UdpMessage ReceiveMessage(byte[] message)
        {
            UdpMessage msg = new UdpMessage();        
            msg.Type = (MessageType) message[0];
            msg.ConnectionId = (short)((message[1] << 8) + message[2]);
            msg.Channel = (short) ((message[3] << 8) + message[4]);
            msg.MessageId = (short) ((message[5] << 8) + message[6]);
            msg.Sequence = (short) ((message[7] << 8) + message[8]);

            msg.Payload = new byte[message.Length - HeaderLength];
            Buffer.BlockCopy(message, HeaderLength, msg.Payload, 0, msg.Payload.Length);
            
            return msg;
        }        
    }

    public class UdpMessage
    {
        public MessageType Type { get; set; }
        public short ConnectionId { get; set; }
        public short Channel { get; set; }
        public short MessageId { get; set; }
        public short Sequence { get; set; }
        public byte[] Payload { get; set; }

        public string Message => Encoding.Default.GetString(Payload);

        public UdpMessage()
        {
        }

        public UdpMessage(MessageType type, short connectionId, short channel, short messageId, short sequence, byte[] payload)
        {
            Type = type;
            ConnectionId = connectionId;
            Channel = channel;
            MessageId = messageId;
            Sequence = sequence;
            Payload = payload;

        }

        public UdpMessage(MessageType type, short connectionId, short channel, short messageId, short sequence, string message)
            : this(type, connectionId, channel, messageId, sequence, Encoding.Default.GetBytes(message))
        {

        }

        public byte[] Bytes => Messaging.CreateMessage(this);
    }
}
