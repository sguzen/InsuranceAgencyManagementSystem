using Microsoft.AspNetCore.Http;

namespace IAMS.Api.Validation
{
    /// <summary>
    /// Validates uploaded policy-import files before they reach the import pipeline:
    /// allow-listed extensions, a size cap, and magic-byte checks so a renamed
    /// executable or archive is rejected regardless of its extension or Content-Type.
    /// </summary>
    public static class ImportFileValidator
    {
        public const long MaxFileSizeBytes = 20 * 1024 * 1024; // 20 MB

        private static readonly string[] AllowedExtensions = { ".xlsx", ".xls", ".csv" };

        // .xlsx is a ZIP container (PK\x03\x04); .xls is an OLE2 compound document.
        private static readonly byte[] ZipMagic = { 0x50, 0x4B, 0x03, 0x04 };
        private static readonly byte[] Ole2Magic = { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 };

        /// <summary>
        /// Returns null when the file is acceptable, otherwise a user-facing error message.
        /// </summary>
        public static async Task<string?> ValidateAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return "No file uploaded";
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return $"File is too large (max {MaxFileSizeBytes / (1024 * 1024)} MB)";
            }

            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            {
                return "Unsupported file type. Allowed: .xlsx, .xls, .csv";
            }

            var header = new byte[8];
            int read;
            await using (var stream = file.OpenReadStream())
            {
                read = await stream.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false);
            }

            switch (extension)
            {
                case ".xlsx" when !StartsWith(header, read, ZipMagic):
                    return "File content does not match the .xlsx format";
                case ".xls" when !StartsWith(header, read, Ole2Magic):
                    return "File content does not match the .xls format";
                case ".csv":
                    // CSV has no magic bytes; reject files that start like a known binary format.
                    if (StartsWith(header, read, ZipMagic) || StartsWith(header, read, Ole2Magic) || header.Take(read).Contains((byte)0))
                    {
                        return "File content does not look like a CSV file";
                    }
                    break;
            }

            return null;
        }

        private static bool StartsWith(byte[] buffer, int available, byte[] magic)
        {
            if (available < magic.Length)
            {
                return false;
            }

            for (var i = 0; i < magic.Length; i++)
            {
                if (buffer[i] != magic[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
