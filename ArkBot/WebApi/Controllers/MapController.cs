using ArkBot.Ark;
using ArkBot.Data;
using ArkBot.ViewModel;
using ArkBot.WebApi.Model;
using ArkSavegameToolkitNet.Domain;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace ArkBot.WebApi.Controllers
{
    /// <summary>
    /// Supplies map images
    /// </summary>
    public class MapController : BaseApiController
    {
        public MapController(IConfig config) : base(config)
        {
        }

        /// <param name="id">MapName</param>
        /// <returns></returns>
        public HttpResponseMessage Get(string id)
        {
            Bitmap bmp = null;
            try
            {
                bmp = MapResources.ResourceManager.GetObject($"topo_map_{id}") as Bitmap;
            }
            catch (MissingManifestResourceException) { }
            catch (MissingSatelliteAssemblyException) { }

            if (bmp == null) return new HttpResponseMessage(HttpStatusCode.NotFound) { ReasonPhrase = $@"Map ""{id}"" does not exist!" };

            var ms = new MemoryStream();

            try
            {
                var jpegEncoder = ImageCodecInfo.GetImageEncoders().FirstOrDefault(x => x.FormatID == ImageFormat.Jpeg.Guid);
                var encParams = new EncoderParameters { Param = new[] { new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 85L) } };
                if (jpegEncoder == null) return new HttpResponseMessage(HttpStatusCode.InternalServerError) { ReasonPhrase = "Could not find jpeg encoder." };

                bmp.Save(ms, jpegEncoder, encParams);
                ms.Seek(0, SeekOrigin.Begin);
            }
            finally
            {
                bmp.Dispose();
            }

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(ms)
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

            return result;
        }
    }
}
