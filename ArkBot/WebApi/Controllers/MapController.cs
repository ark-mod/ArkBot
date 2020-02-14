using ArkBot.Configuration.Model;
using ArkSavegameToolkitNet;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

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
            var notfound = new HttpResponseMessage(HttpStatusCode.NotFound) { ReasonPhrase = $@"Map ""{id}"" does not exist!" };
            Bitmap bmp = null;
            var ms = new MemoryStream();
            try
            {
                if (!ArkToolkitSettings.Instance.Maps.TryGetValue(id, out var def)) return notfound;

                bmp = def.Images?.FirstOrDefault()?.ImageProvider?.Invoke();

                if (bmp == null) return notfound;

                var jpegEncoder = ImageCodecInfo.GetImageEncoders().FirstOrDefault(x => x.FormatID == ImageFormat.Jpeg.Guid);
                var encParams = new EncoderParameters { Param = new[] { new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 85L) } };
                if (jpegEncoder == null) return new HttpResponseMessage(HttpStatusCode.InternalServerError) { ReasonPhrase = "Could not find jpeg encoder." };

                bmp.Save(ms, jpegEncoder, encParams);
                ms.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception) { return notfound; }
            finally
            {
                bmp?.Dispose();
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