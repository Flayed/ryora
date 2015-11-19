using NUnit.Core;
using NUnit.Framework;

namespace Ryora.Udp.UnitTests
{
    [TestFixture]
    public class MessageConverterTests
    {
        [Test]
        [TestCase(new bool[] { false, false, false, false, false, false, false, false }, 0)]
        [TestCase(new bool[] { true, false, false, false, false, false, false, false }, 1)]
        [TestCase(new bool[] { false, true, false, false, false, false, false, false }, 2)]
        [TestCase(new bool[] { true, true, false, false, false, false, false, false }, 3)]
        [TestCase(new bool[] { false, false, true, false, false, false, false, false }, 4)]
        [TestCase(new bool[] { true, false, true, false, false, false, false, false }, 5)]
        [TestCase(new bool[] { false, false, false, true, false, false, false, false }, 8)]
        [TestCase(new bool[] { false, false, false, false, true, false, false, false }, 16)]
        [TestCase(new bool[] { false, false, false, false, false, true, false, false }, 32)]
        [TestCase(new bool[] { false, false, false, false, false, false, true, false }, 64)]
        [TestCase(new bool[] { false, false, false, false, false, false, false, true }, 128)]
        [TestCase(new bool[] { true, true, true, true, true, true, true, true }, 255)]
        public void Payloader_BooleanArray_PacksInToOneByte(bool[] booleanArray, int expectedValue)
        {
            var actual = MessageConverter.Payloader(booleanArray);
            Assert.AreEqual((byte)expectedValue, actual[0]);
        }

        [Test]
        [TestCase(0, new bool[] { false, false, false, false, false, false, false, false })]
        [TestCase(1, new bool[] { true, false, false, false, false, false, false, false })]
        [TestCase(2, new bool[] { false, true, false, false, false, false, false, false })]
        [TestCase(3, new bool[] { true, true, false, false, false, false, false, false })]
        [TestCase(4, new bool[] { false, false, true, false, false, false, false, false })]
        [TestCase(5, new bool[] { true, false, true, false, false, false, false, false })]
        [TestCase(8, new bool[] { false, false, false, true, false, false, false, false })]
        [TestCase(16, new bool[] { false, false, false, false, true, false, false, false })]
        [TestCase(32, new bool[] { false, false, false, false, false, true, false, false })]
        [TestCase(64, new bool[] { false, false, false, false, false, false, true, false })]
        [TestCase(128, new bool[] { false, false, false, false, false, false, false, true })]
        [TestCase(255, new bool[] { true, true, true, true, true, true, true, true })]
        public void ReadBoolArray_SingleByte_ProducesBooleanArray(byte b, bool[] booleanArray)
        {
            var offset = 0;
            var bitArray = MessageConverter.ReadBoolArray(new byte[] { b }, ref offset);

            for (var idx = 0; idx < booleanArray.Length && idx < bitArray.Length; idx++)
            {
                Assert.AreEqual(booleanArray[idx], bitArray[idx], $"{idx} failed: {booleanArray[idx]} != {bitArray[idx]}");
            }
        }
    }
}
