 using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;


namespace ImageResizer.Controllers
{
    public partial class ImagesController: ControllerBase
    {
        [Route("api/images")]
        public async Task<IActionResult> GetImages(
            [FromQuery]string name,
            [FromServices]IWebHostEnvironment env
        )
        {
            var filePath = Path.Combine(env.ContentRootPath, "images", name);
            var fileExists = System.IO.File.Exists(filePath);

            if(! fileExists)
            {
                return NotFound();
            }

            var file = await System.IO.File.ReadAllBytesAsync(filePath);

            return File(file, "image/jpeg");

        }    
    }
}