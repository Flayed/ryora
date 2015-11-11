using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ryora.Tech.Services
{
    interface IRealtimeService
    {
        event EventHandler NewImage;
        event EventHandler MouseMove;
        event EventHandler Sharing;

        Task StartConnection(short channel);
        string Transport { get; }
    }
}
