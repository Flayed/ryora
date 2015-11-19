namespace Ryora.UdpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var udpService = new UdpService();
            while (true)
            {
                udpService.Listen().Wait();
            }
        }
    }
}
