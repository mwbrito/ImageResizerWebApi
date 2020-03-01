using System.IO;
using System.Threading.Tasks;
using ImageMagick;
using ImageResizer.Converter;
using ImageResizer.Models;
using ImageResizer.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

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

            (var fileExists, var blobFile) = await storage.TryGetFile(requestModel.Name);
            if(! fileExists)
            {
                return NotFound();
            }

            var options = ConversionOptionsFactory.FromResizeRequest(requestModel);
            var imageSource = await storage.GetBlobBytes(blobFile);
            var result = await converter.Convert(imageSource, options);

            if(result.Length ==0)
            {
                return BadRequest("Couldn't convert file.");
            }

            return File(result, options.TargetMimeType);

        }    
    }
}