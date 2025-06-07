using System.Net;
using Common;
using Domain.Common;
using Domain.Dtos.Cdn;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Api.Controllers;

[Route("api/v1/cdn")]
public class CdnController : BaseApiController
{
    private readonly IS3Service _s3Service;
    private readonly IConfiguration _configuration;

    public CdnController(
        IS3Service s3Service, 
        IConfiguration configuration,
        IHttpContextAccessor contextAccessor) 
        :base(contextAccessor)
    {
        _s3Service = s3Service;
        _configuration = configuration;
    }

    [HttpPost("file")]
    public async Task<ActionResult<ApiResponse<UploadFileResponse>>> UploadFile([FromForm] IFormFile file,[BindRequired] string container)
    {
        if (file == null || file.Length == 0)
            return StatusCode(400, "Error uploading file to S3");
        
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = Guid.NewGuid().ToString();
        var bucketName = _configuration["AWS:BucketName"];
        var key = $"{container}/{fileName}{extension}";
        
        if (container == StorageConstants.Profile)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            
            if (!allowedExtensions.Contains(extension))
                return StatusCode(400, "Invalid file type. Only JPG, JPEG, PNG and GIF are allowed.");

            long maxSizeInBytes = 2 * 1024 * 1024;

            if (file.Length > maxSizeInBytes)
                return StatusCode(400, "Dosya boyutu 2MB üzeri olamaz");
            
            try
            {
                using var stream = file.OpenReadStream();
                var fileSendUrl = await _s3Service.UploadFileAsync(bucketName, key, stream, file.ContentType);
                var fileUrl = FileFormatter.GetFile(container, fileName,extension);

                var cdnResponse = new UploadFileResponse
                {
                    FileSendUrl = fileSendUrl,
                    Name = fileName,
                    Url = fileUrl
                };

                return Success(cdnResponse, "Dosya başarıyla yüklendi.", HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading file: {ex.Message}");
            }
        }

        return StatusCode(400, "Hatalı bir işlem yaptınız.");
    }
    
    
}