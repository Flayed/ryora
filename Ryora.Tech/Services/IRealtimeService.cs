using System;
using System.Threading.Tasks;

namespace Ryora.Tech.Services
{
    interface IRealtimeService
    {
        event EventHandler NewImage;
        event EventHandler MouseMove;
        event EventHandler Sharing;

        Task StartConnection(short channel);
        Task EndConnection(short channel);
        string Transport { get; }
    }
}
