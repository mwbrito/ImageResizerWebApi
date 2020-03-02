using System.Threading.Tasks;
using ImageResizer.Converter;
using ImageResizer.Models;
using ImageResizer.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace ImageResizer.Controllers
{
    public partial class ImagesController: ControllerBase
    {
        [Route("api/images/resize")]
        public async Task<IActionResult> ResizeImage(
            [FromQuery]ResizeRequestModel requestModel,
            [FromServices]IWebHostEnvironment env,
            [FromServices]StorageService storage,
            [FromServices]ImageConverter converter
        )
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var options = ConversionOptionsFactory.FromResizeRequest(requestModel);

            (var cacheExists, var cacheFile) = await storage.TryGetFileCached(requestModel.Name);
            if(cacheExists)
            {
                if(IsEtagNotModified(Request, cacheFile.Properties.ETag))
                {
                    return new NotModifiedResult(cacheFile.Properties.LastModified.GetValueOrDefault().UtcDateTime, cacheFile.Properties.ETag);
                }
                var cacheContent = await storage.GetBlobBytes(cacheFile);
                return File(cacheContent, cacheFile.Properties.ContentType, cacheFile.Properties.LastModified.GetValueOrDefault().UtcDateTime, new EntityTagHeaderValue(cacheFile.Properties.ETag));
            }

            (var fileExists, var blobFile) = await storage.TryGetFile(requestModel.Name);
            if(! fileExists)
            {
                return NotFound();
            }
            
            var imageSource = await storage.GetBlobBytes(blobFile);
            var result = await converter.Convert(imageSource, options);

            if(result.Length ==0)
            {
                return BadRequest("Couldn't convert file.");
            }

            (var uplaodOk, var savedFile) = await storage.TryUploadToCache(options.GetCacheKey(), result, options.TargetMimeType);

            return File(result, savedFile.Properties.ContentType, cacheFile.Properties.LastModified.GetValueOrDefault().UtcDateTime, new EntityTagHeaderValue(cacheFile.Properties.ETag));
        }

        static bool IsEtagNotModified(HttpRequest request, string etag)
        {
            var requestHeaders = request ? .GetTypedHeaders();

            return requestHeaders?.IfNoneMatch != null && requestHeaders.IfNoneMatch.Contains(new EntityTagHeaderValue(etag));
        }
    }
}