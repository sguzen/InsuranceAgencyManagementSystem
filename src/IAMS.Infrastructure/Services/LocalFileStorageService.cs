// IAMS.Infrastructure/Services/LocalFileStorageService.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using IAMS.Infrastructure.Interfaces;

namespace IAMS.Infrastructure.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<LocalFileStorageService> _logger;
        private readonly string _basePath;
        private readonly string _baseUrl;

        public LocalFileStorageService(IConfiguration configuration, ILogger<LocalFileStorageService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _basePath = _configuration["FileStorage:LocalPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            _baseUrl = _configuration["FileStorage:BaseUrl"] ?? "/files";

            // Ensure base directory exists
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName, string? folder = null)
        {
            try
            {
                // Sanitize file name
                var sanitizedFileName = SanitizeFileName(fileName);
                var uniqueFileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}_{sanitizedFileName}";

                // Create folder path if specified
                var folderPath = folder != null ? Path.Combine(_basePath, folder) : _basePath;
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var filePath = Path.Combine(folderPath, uniqueFileName);
                var relativePath = folder != null ? Path.Combine(folder, uniqueFileName) : uniqueFileName;

                // Save file
                using var fileStreamOutput = File.Create(filePath);
                await fileStream.CopyToAsync(fileStreamOutput);

                _logger.LogInformation("File uploaded successfully: {FilePath}", relativePath);
                return relativePath.Replace('\\', '/'); // Normalize path separators
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file: {FileName}", fileName);
                throw;
            }
        }

        public async Task<Stream> DownloadAsync(string filePath)
        {
            try
            {
                var fullPath = GetFullPath(filePath);

                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }

                var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                return await Task.FromResult<Stream>(fileStream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download file: {FilePath}", filePath);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string filePath)
        {
            try
            {
                var fullPath = GetFullPath(filePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
                    return await Task.FromResult(true);
                }

                return await Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file: {FilePath}", filePath);
                return false;
            }
        }

        public async Task<bool> ExistsAsync(string filePath)
        {
            try
            {
                var fullPath = GetFullPath(filePath);
                return await Task.FromResult(File.Exists(fullPath));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking file existence: {FilePath}", filePath);
                return false;
            }
        }

        public async Task<List<StoredFile>> ListFilesAsync(string? folder = null)
        {
            try
            {
                var searchPath = folder != null ? Path.Combine(_basePath, folder) : _basePath;

                if (!Directory.Exists(searchPath))
                {
                    return await Task.FromResult(new List<StoredFile>());
                }

                var files = Directory.GetFiles(searchPath, "*", SearchOption.TopDirectoryOnly)
                    .Select(fullPath =>
                    {
                        var fileInfo = new FileInfo(fullPath);
                        var relativePath = Path.GetRelativePath(_basePath, fullPath).Replace('\\', '/');

                        return new StoredFile
                        {
                            Name = fileInfo.Name,
                            Path = relativePath,
                            Size = fileInfo.Length,
                            CreatedDate = fileInfo.CreationTimeUtc,
                            ContentType = GetContentType(fileInfo.Extension)
                        };
                    })
                    .ToList();

                return await Task.FromResult(files);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list files in folder: {Folder}", folder);
                return new List<StoredFile>();
            }
        }

        public async Task<string> GetPublicUrlAsync(string filePath)
        {
            try
            {
                // For local storage, return a URL that can be served by the web server
                var url = $"{_baseUrl}/{filePath.Replace('\\', '/')}";
                return await Task.FromResult(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get public URL for file: {FilePath}", filePath);
                throw;
            }
        }

        public async Task<long> GetFileSizeAsync(string filePath)
        {
            try
            {
                var fullPath = GetFullPath(filePath);

                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }

                var fileInfo = new FileInfo(fullPath);
                return await Task.FromResult(fileInfo.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get file size: {FilePath}", filePath);
                throw;
            }
        }

        private string GetFullPath(string relativePath)
        {
            return Path.Combine(_basePath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        }

        private static string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
            return string.IsNullOrEmpty(sanitized) ? "file" : sanitized;
        }

        private static string GetContentType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                ".zip" => "application/zip",
                _ => "application/octet-stream"
            };
        }
    }

    // Alternative Azure Blob Storage implementation
    public class AzureBlobStorageService : IFileStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureBlobStorageService> _logger;
        private readonly string _connectionString;
        private readonly string _containerName;

        public AzureBlobStorageService(IConfiguration configuration, ILogger<AzureBlobStorageService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _connectionString = _configuration["FileStorage:AzureStorage:ConnectionString"] ?? string.Empty;
            _containerName = _configuration["FileStorage:AzureStorage:ContainerName"] ?? "iams-files";
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName, string? folder = null)
        {
            // Implementation would use Azure.Storage.Blobs
            // This is a placeholder - you would need to add the Azure.Storage.Blobs NuGet package
            throw new NotImplementedException("Azure Blob Storage implementation requires Azure.Storage.Blobs package");
        }

        public async Task<Stream> DownloadAsync(string filePath)
        {
            throw new NotImplementedException("Azure Blob Storage implementation requires Azure.Storage.Blobs package");
        }

        public async Task<bool> DeleteAsync(string filePath)
        {
            throw new NotImplementedException("Azure Blob Storage implementation requires Azure.Storage.Blobs package");
        }

        public async Task<bool> ExistsAsync(string filePath)
        {
            throw new NotImplementedException("Azure Blob Storage implementation requires Azure.Storage.Blobs package");
        }

        public async Task<List<StoredFile>> ListFilesAsync(string? folder = null)
        {
            throw new NotImplementedException("Azure Blob Storage implementation requires Azure.Storage.Blobs package");
        }

        public async Task<string> GetPublicUrlAsync(string filePath)
        {
            throw new NotImplementedException("Azure Blob Storage implementation requires Azure.Storage.Blobs package");
        }

        public async Task<long> GetFileSizeAsync(string filePath)
        {
            throw new NotImplementedException("Azure Blob Storage implementation requires Azure.Storage.Blobs package");
        }
    }
}