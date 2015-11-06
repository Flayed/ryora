using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Routing;
using Ryora.Server.Hubs.Accessors;
using Ryora.Server.Services;

namespace Ryora.Server.API
{
    [RoutePrefix("API/RA"), AllowAnonymous]
    public class RemoteAssistApiController : ApiController
    {
        [Route("{id:int}/{frame:int}"), HttpPost]
        public async Task PostImage(int id, int frame)
        {
            var content = await Request.Content.ReadAsByteArrayAsync();
            await RemoteAssistService.AddImage(id.ToString(), frame, Convert.ToBase64String(content));
        }

        [Route("GetImage"), HttpPost]
        public async Task<HttpResponseMessage> GetImage()
        {
            var guid = new Guid(await Request.Content.ReadAsStringAsync());
            var imageData = RemoteAssistService.GetImage(guid);
            var content = new ByteArrayContent(imageData);
            var response = new HttpResponseMessage(HttpStatusCode.OK) {Content = content};
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");            
            return response;
        }
    }
}
