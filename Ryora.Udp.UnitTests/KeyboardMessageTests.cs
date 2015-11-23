using NUnit.Framework;
using Ryora.Udp.Messages;
using System.Linq;
using System.Windows.Input;

namespace Ryora.Udp.UnitTests
{
    [TestFixture]
    public class KeyboardMessageTests
    {
        [Test]
        public void ToPayload_SingleKeystroke_PayloadIs3Bytes()
        {
            var expectedLength = 3;
            KeyboardMessage message = new KeyboardMessage(true, (short)Key.A);
            var payload = message.ToPayload();

            Assert.AreEqual(expectedLength, payload.Length);
        }

        [Test]
        [TestCase(true, 1, new[] { (short)Key.A })]
        [TestCase(false, 1, new[] { (short)Key.A })]
        [TestCase(true, 5, new[] { (short)Key.A, (short)Key.B, (short)Key.C, (short)Key.D, (short)Key.E })]
        [TestCase(false, 5, new[] { (short)Key.A, (short)Key.B, (short)Key.C, (short)Key.D, (short)Key.E })]
        public void KeyboardMessage_FromByteArray_MetabyteDefined(bool isDown, int expectedCount, short[] keys)
        {
            KeyboardMessage original = new KeyboardMessage(isDown, keys);
            Assert.AreEqual(expectedCount, original.NumberOfKeys);
            var payload = original.ToPayload();

            var actual = new KeyboardMessage(payload);

            Assert.AreEqual(original.IsDown, actual.IsDown);
            Assert.AreEqual(original.NumberOfKeys, actual.NumberOfKeys);
            for (var idx = 0; idx < original.NumberOfKeys; idx++)
            {
                Assert.AreEqual(original.Keys.ElementAt(idx), actual.Keys.ElementAt(idx));
            }
        }
    }
}
