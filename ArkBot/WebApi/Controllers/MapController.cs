using ArkBot.Configuration.Model;
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
            string imageExtension;
            var ms = new MemoryStream();
            try
            {
                var map = GetMapImage(id);
                bmp = map.Value;
                imageExtension = map.Key;
                if (bmp == null || string.IsNullOrWhiteSpace(imageExtension)) return notfound;

                var imageFormat = GetImageFormatGuid(imageExtension);
                var imageEncoder = ImageCodecInfo.GetImageEncoders().FirstOrDefault(x => x.FormatID == imageFormat);
                var encParams = new EncoderParameters { Param = new[] { new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 85L) } };
                if (imageEncoder == null) return new HttpResponseMessage(HttpStatusCode.InternalServerError) { ReasonPhrase = "Could not find " + imageExtension + " encoder." };

                bmp.Save(ms, imageEncoder, encParams);
                ms.Seek(0, SeekOrigin.Begin);
            }
            catch (MissingManifestResourceException)
            {
                return notfound;
            }
            catch (MissingSatelliteAssemblyException)
            {
                return notfound;
            }
            finally
            {
                bmp?.Dispose();
            }

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(ms)
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/" + imageExtension);

            return result;
        }

        private static Guid GetImageFormatGuid(string imageExtension)
        {
            switch (imageExtension)
            {
                case "jpeg":
                    return ImageFormat.Jpeg.Guid;
                case "jpg":
                    return ImageFormat.Jpeg.Guid;
                case "bmp":
                    return ImageFormat.Bmp.Guid;
                case "png":
                    return ImageFormat.Png.Guid;
                default:
                    return ImageFormat.Jpeg.Guid;
            }
        }

        private static KeyValuePair<string, Bitmap> GetMapImage(string imageName)
        {
            var imageDirectory = @"Resources\MapImages";
            imageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imageDirectory);
            var filePath = Directory.GetFiles(imageDirectory).FirstOrDefault(x => Path.GetFileNameWithoutExtension(x) == imageName);
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                Bitmap bitmap;
                using (Stream bmpStream = System.IO.File.Open(filePath, System.IO.FileMode.Open))
                {
                    Image image = Image.FromStream(bmpStream);

                    bitmap = new Bitmap(image);
                }

                var imageExtension = Path.GetExtension(filePath);

                return new KeyValuePair<string, Bitmap>(imageExtension, bitmap);
            }

            return new KeyValuePair<string, Bitmap>(null, null);
        }
    }
}