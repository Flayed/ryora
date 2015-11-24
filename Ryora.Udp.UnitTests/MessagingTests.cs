using NUnit.Core;
using NUnit.Framework;
using Ryora.Udp.Messages;
using System.Collections;

namespace Ryora.Udp.UnitTests
{
    [TestFixture]
    public class MessagingTests
    {
        [Test]
        public void Messaging_Header_EncodesAndDecodes()
        {
            MessageType expectedMessageType = MessageType.Disconnect;
            short expectedClientId = 12345;
            short expectedChannel = 42;
            short expectedMessageId = 12;

            var expectedMessage = Messaging.CreateMessage(expectedMessageType, expectedClientId, expectedChannel, expectedMessageId);

            var actualMessage = Messaging.ReceiveMessage(expectedMessage);

            Assert.AreEqual(expectedMessageType, actualMessage.Type);
            Assert.AreEqual(expectedClientId, actualMessage.ConnectionId);
            Assert.AreEqual(expectedChannel, actualMessage.Channel);
            Assert.AreEqual(expectedMessageId, actualMessage.MessageId);
        }

        public IEnumerable MessagingMessageTests
        {
            get
            {
                short expectedClientId = 12345;
                short expectedChannel = 42;
                short expectedMessageId = 12;
                string testName = "Messaging_$TestName_EncodesAndDecodesHeader";
                yield return
                    new TestCaseData(new AcknowledgeMessage(1920, 1080), expectedClientId, expectedChannel,
                        expectedMessageId).SetName(testName.Replace("$TestName", "AcknowledgeMessage"));
                yield return
                    new TestCaseData(new ConnectMessage(1920, 1080), expectedClientId, expectedChannel,
                        expectedMessageId).SetName(testName.Replace("$TestName", "ConnectMessage"));
                yield return
                    new TestCaseData(new ImageMessage(0, 0, 0, 0, 0, 0, new byte[0]), expectedClientId, expectedChannel,
                        expectedMessageId).SetName(testName.Replace("$TestName", "ImageMessage"));
                yield return
                    new TestCaseData(new KeyboardMessage(true, 1), expectedClientId, expectedChannel, expectedMessageId)
                        .SetName(testName.Replace("$TestName", "KeyboardMessage"));
                yield return
                    new TestCaseData(new MouseMessage(0, 0, 0, 0, 0), expectedClientId, expectedChannel,
                        expectedMessageId).SetName(testName.Replace("$TestName", "MouseMessage"));
                yield return
                    new TestCaseData(new SharingMessage(false), expectedClientId, expectedChannel, expectedMessageId)
                        .SetName(testName.Replace("$TestName", "SharingMessage"));
            }
        }

        [Test]
        [TestCaseSource(typeof(MessagingTests), nameof(MessagingMessageTests))]
        public void Messaging_Message_EncodesAndDecodesHeader(IMessage message, short expectedClientId, short expectedChannel, short expectedMessageId)
        {
            var expectedMessage = Messaging.CreateMessage(message, expectedClientId, expectedChannel, expectedMessageId);

            var actualMessage = Messaging.ReceiveMessage(expectedMessage);

            Assert.AreEqual(message.MessageType, actualMessage.Type);
            Assert.AreEqual(expectedClientId, actualMessage.ConnectionId);
            Assert.AreEqual(expectedChannel, actualMessage.Channel);
            Assert.AreEqual(expectedMessageId, actualMessage.MessageId);
        }
    }
}
