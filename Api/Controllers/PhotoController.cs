using Business.Services;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using Domain.Common;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/v1/photo")]
    public class PhotoController : BaseApiController
    {
        private readonly IS3Service _s3Service;
        private readonly IConfiguration _configuration;

        public PhotoController(
            IS3Service s3Service,
            IConfiguration configuration,
            IHttpContextAccessor contextAccessor) 
            : base(contextAccessor)
        {
            _s3Service = s3Service;
            _configuration = configuration;
        }
        
        /*
        [HttpPost("upload")]
        public async Task<ActionResult<ApiResponse>> UploadPhoto([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Failure("No file uploaded");

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return Failure("Invalid file type. Only JPG, JPEG, PNG and GIF are allowed.");

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                return Failure("File size exceeds 5MB limit");

            var bucketName = _configuration["AWS:BucketName"];
            // Sanitize filename and create key
            var sanitizedFileName = Path.GetFileNameWithoutExtension(file.FileName)
                .Replace(" ", "-")
                .Replace("+", "-")
                .Replace("&", "-")
                .Replace("?", "-")
                .Replace("=", "-")
                .Replace("#", "-");
            var key = $"profile-pictures/{Guid.NewGuid()}-{sanitizedFileName}{extension}";

            try
            {
                using var stream = file.OpenReadStream();
                var fileUrl = await _s3Service.UploadFileAsync(bucketName, key, stream, file.ContentType);
                
                return Ok(new { 
                    url = fileUrl,
                    key = key
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading file: {ex.Message}");
            }
        }
        
        */

        [HttpGet("get/{folder}/{filename}")]
        public async Task<IActionResult> GetPhoto(string folder, string filename)
        {
            var bucketName = _configuration["AWS:BucketName"];
            var key = $"{folder}/{filename}";

            try
            {
                var stream = await _s3Service.GetFileAsync(bucketName, key);
                var extension = Path.GetExtension(filename).ToLowerInvariant();
                var contentType = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    _ => "application/octet-stream"
                };

                return File(stream, contentType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving file: {ex.Message}");
            }
        }

        [HttpDelete("delete/{folder}/{filename}")]
        public async Task<IActionResult> DeletePhoto(string folder, string filename)
        {
            var bucketName = _configuration["AWS:BucketName"];
            var key = $"{folder}/{filename}";

            try
            {
                await _s3Service.DeleteFileAsync(bucketName, key);
                return Ok(new { message = "File deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting file: {ex.Message}");
            }
        }

        [HttpGet("url/{folder}/{filename}")]
        public IActionResult GetPhotoUrl(string folder, string filename)
        {
            var bucketName = _configuration["AWS:BucketName"];
            var key = $"{folder}/{filename}";
            var url = _s3Service.GetFileUrl(bucketName, key);
            return Ok(new { url });
        }
    }
} 