using Microsoft.AspNetCore.Mvc;
using ChatService.Web.Dtos;
using ChatService.Web.Storage;


namespace ChatService.Web.Controllers
{
    [Route("api/[controller]")]
    public class ImagesController : Controller
    {
        private readonly IImageStore _imageStore;

        public ImagesController(IImageStore imageStore)
        {
            _imageStore = imageStore;
        }

        [HttpPost]
        public async Task<ActionResult<UploadImageResponse>> UploadImage([FromForm] UploadImageRequest request)
        {
            using var stream = new MemoryStream();
            await request.File.CopyToAsync(stream);
            string imageId = await _imageStore.Upload(stream.ToArray());
            return CreatedAtAction(nameof(DownloadImage),
                new { Id = imageId }, new UploadImageResponse(imageId));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> DownloadImage(string id)
        {
            byte[] bytes = await _imageStore.Download(id);
            if (bytes == null)
            {
                return NotFound("");    
            }
            
            return new FileContentResult(bytes, "application/octet-stream");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(string id)
        {
            await _imageStore.Delete(id);
            return Ok();
        }
    }
}