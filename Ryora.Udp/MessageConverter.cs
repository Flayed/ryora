using System;
using System.Collections.Generic;
using System.Text;

namespace Ryora.Udp
{
    public static class MessageConverter
    {
        public static byte[] Payloader(params object[] args)
        {
            var byteList = new List<byte>();
            foreach (var arg in args)
            {
                List<byte> bytes = new List<byte>();
                if (arg is bool)
                    bytes.AddRange(BitConverter.GetBytes((bool)arg));
                else if (arg is char)
                    bytes.AddRange(BitConverter.GetBytes((char)arg));
                else if (arg is double)
                    bytes.AddRange(BitConverter.GetBytes((double)arg));
                else if (arg is float)
                    bytes.AddRange(BitConverter.GetBytes((float)arg));
                else if (arg is int)
                    bytes.AddRange(BitConverter.GetBytes((int)arg));
                else if (arg is long)
                    bytes.AddRange(BitConverter.GetBytes((long)arg));
                else if (arg is short)
                    bytes.AddRange(BitConverter.GetBytes((short)arg));
                else if (arg is uint)
                    bytes.AddRange(BitConverter.GetBytes((uint)arg));
                else if (arg is ulong)
                    bytes.AddRange(BitConverter.GetBytes((ulong)arg));
                else if (arg is ushort)
                    bytes.AddRange(BitConverter.GetBytes((ushort)arg));
                else if (arg is MessageType)
                    bytes.Add((byte)((int)(MessageType)arg));
                else if (arg is string)
                    bytes.AddRange(Encoding.Default.GetBytes((string)arg));
                else if (arg is byte[])
                    bytes.AddRange((byte[])arg);

                byteList.AddRange(bytes);
            }
            return byteList.ToArray();
        }

        public static bool ReadBool(byte[] data, ref int offset)
        {
            var s = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            return s;
        }

        public static char ReadChar(byte[] data, ref int offset)
        {
            var s = BitConverter.ToChar(data, offset);
            offset += sizeof(char);
            return s;
        }

        public static double ReadDouble(byte[] data, ref int offset)
        {
            var s = BitConverter.ToDouble(data, offset);
            offset += sizeof(double);
            return s;
        }

        public static float ReadFloat(byte[] data, ref int offset)
        {
            var s = BitConverter.ToSingle(data, offset);
            offset += sizeof(float);
            return s;
        }

        public static int ReadInt(byte[] data, ref int offset)
        {
            var s = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            return s;
        }

        public static long ReadLong(byte[] data, ref int offset)
        {
            var s = BitConverter.ToInt64(data, offset);
            offset += sizeof(long);
            return s;
        }

        public static ulong ReadULong(byte[] data, ref int offset)
        {
            var s = BitConverter.ToUInt64(data, offset);
            offset += sizeof(ulong);
            return s;
        }

        public static ushort ReadUShort(byte[] data, ref int offset)
        {
            var s = BitConverter.ToUInt16(data, offset);
            offset += sizeof(ushort);
            return s;
        }

        public static short ReadShort(byte[] data, ref int offset)
        {
            var s = BitConverter.ToInt16(data, offset);
            offset += sizeof(short);
            return s;
        }

        public static uint ReadUInt(byte[] data, ref int offset)
        {
            var s = BitConverter.ToUInt32(data, offset);
            offset += sizeof(uint);
            return s;
        }
    }
}
