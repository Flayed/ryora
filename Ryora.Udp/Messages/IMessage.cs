namespace Ryora.Udp.Messages
{
    public interface IMessage
    {
        MessageType MessageType { get; }
        byte[] ToPayload();
    }
}
