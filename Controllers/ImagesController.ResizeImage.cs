using System.IO;
using System.Threading.Tasks;
using ImageMagick;
using ImageResizer.Converter;
using ImageResizer.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;


namespace ImageResizer.Controllers
{
    public partial class ImagesController: ControllerBase
    {
        [Route("api/images/resize")]
        public async Task<IActionResult> ResizeImage(
            [FromQuery]ResizeRequestModel requestModel,
            [FromServices]IHostingEnvironment env
        )
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var filePath = Path.Combine(env.ContentRootPath, "images", requestModel.Name);
            var fileExists = System.IO.File.Exists(filePath);
            if(! fileExists)
            {
                return NotFound();
            }

            var options = ConversionOptionsFactory.FromResizeRequest(requestModel);

            using (var memory = new MemoryStream())
            using(var image = new MagickImage(filePath))
            {
                image.Resize(options.Width, options.Height);
                image.Strip();
                image.Quality = options.Quality;
                image.Format = options.TargetFormat;

                image.Write(memory);
                var file = memory.ToArray();
                return File(file, options.TargetMimeType);
            }

        }    
    }
}