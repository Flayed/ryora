using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace Ryora.Client.Services
{
    public interface IRealtimeService
    {
        Task StartConnection(short channel);
        Task SendImage(short channel, int frame, byte[] image);
        Task SendMouseCoords(short channel, double x, double y);
        Task Sharing(short channel, bool isSharing);
    }
}
