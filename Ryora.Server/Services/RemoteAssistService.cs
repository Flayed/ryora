using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ryora.Server.Hubs.Accessors;

namespace Ryora.Server.Services
{
    public static class RemoteAssistService
    {
        private static readonly Dictionary<Guid, byte[]> ImageCache = new Dictionary<Guid, byte[]>();

        public static async Task AddImage(string channel, int frame, string imageData)
        {
            //var guid = Guid.NewGuid();
            //ImageCache.Add(guid, imageData);
            await RemoteAssistHubAccessor.PublishImage(channel, frame, imageData);
            //return guid;
        }

        public static byte[] GetImage(Guid guid)
        {
            var imageData = ImageCache[guid];
            if (imageData != null)
            {
                ImageCache.Remove(guid);                
            }
            return imageData;
        }

        public static byte[] GetImage(string guid)
        {
            return GetImage(new Guid(guid));
        }
    }
}

