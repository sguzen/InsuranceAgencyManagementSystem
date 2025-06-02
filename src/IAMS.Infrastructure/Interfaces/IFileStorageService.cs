using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Infrastructure.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadAsync(Stream fileStream, string fileName, string? folder = null);
        Task<Stream> DownloadAsync(string filePath);
        Task<bool> DeleteAsync(string filePath);
        Task<bool> ExistsAsync(string filePath);
        Task<List<StoredFile>> ListFilesAsync(string? folder = null);
        Task<string> GetPublicUrlAsync(string filePath);
        Task<long> GetFileSizeAsync(string filePath);
    }

    public class StoredFile
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ContentType { get; set; } = string.Empty;
    }
}
