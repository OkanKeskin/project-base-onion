using System.IO;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IS3Service
    {
        Task<string> UploadFileAsync(string bucketName, string key, Stream fileStream, string contentType);
        Task<Stream> GetFileAsync(string bucketName, string key);
        Task DeleteFileAsync(string bucketName, string key);
        string GetFileUrl(string bucketName, string key);
    }
} 