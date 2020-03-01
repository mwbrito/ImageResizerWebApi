using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ImageMagick;

namespace ImageResizer.Converter
{
    public class ImageConverter
    {
        public async Task<byte[]> Convert(byte[] source, ConversionOptions options)
        {
            switch(options.TargetFormat)
            {
                case MagickFormat.Png24:
                    return await ConvertToPng(source, options);

                default:
                    return await ConvertToJpg(source, options);
            }
        }

        Task<byte[]> ConvertToPng(byte[] source, ConversionOptions options) => Execute(source, $"- -filter Triangle -define filter:support=2 -resize {options.Width}x{options.Height}> -density 150x150 -unsharp 0.25x0.25+8+0.065 -dither None -posterize 136 -define png:compression-filter=5 -define png:compression-level=9 -define png:compression-strategy=1 -define png:exclude-chunk=all -interlace none -colorspace sRGB -strip png:-");

        Task<byte[]> ConvertToJpg(byte[] source, ConversionOptions options) => Execute(source, $"- -filter Triangle -define filter:support=2 -resize {options.Width}x{options.Height}> -unsharp 0.25x0.25+8+0.065 -dither None -posterize 136 -quality {options.Quality} -define jpeg:fancy-upsampling=off -interlace none -colorspace sRGB -background white -alpha remove -strip jpg:-");

        async Task<byte[]> Execute(byte[] source, string arguments)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "convert",
                    Arguments = arguments,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            try
            {
                process.Start();

                using(var input = new MemoryStream(source))
                {
                    input.Position = 0;
                    await input.CopyToAsync(process.StandardInput.BaseStream);
                }

                await process.StandardInput.FlushAsync();
                process.StandardInput.Dispose();

                var memory = new MemoryStream();
                await process.StandardOutput.BaseStream.CopyToAsync(memory);

                process.WaitForExit(10 * 10000);
                if(process.HasExited)
                {
                    return memory.ToArray();
                }

                try
                {
                    process.Kill();
                }
                catch(Exception ex)
                {
                    throw new Exception("Couldn't kill the process after 10 seconds.", ex);
                }

                return memory.ToArray();
            }
            catch(Exception ex)
            {
                var errorOutput = $"Couldnt convert image. '{GetErrorOut(process)}'";
                throw new Exception(errorOutput, ex);
            }
            finally
            {
                process.Dispose();
            }
        }

        static async Task<string> GetErrorOut(Process process)
        {
            try
            {
                return await process.StandardError.ReadToEndAsync();
            }
            catch(Exception)
            {
                return string.Empty;
            }
        }
    }
}