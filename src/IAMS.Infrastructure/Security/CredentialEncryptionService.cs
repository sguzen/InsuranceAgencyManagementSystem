using Microsoft.AspNetCore.DataProtection;

namespace IAMS.Infrastructure.Security
{
    /// <summary>
    /// Service for encrypting and decrypting sensitive credentials.
    /// Uses ASP.NET Core Data Protection API for secure encryption.
    /// </summary>
    public interface ICredentialEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string encryptedText);
        bool IsEncrypted(string text);
    }

    public class CredentialEncryptionService : ICredentialEncryptionService
    {
        private readonly IDataProtector _protector;
        private const string EncryptedPrefix = "ENC:";
        private const string Purpose = "IAMS.Credentials.v1";

        public CredentialEncryptionService(IDataProtectionProvider dataProtectionProvider)
        {
            _protector = dataProtectionProvider.CreateProtector(Purpose);
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            // Don't double-encrypt
            if (IsEncrypted(plainText))
                return plainText;

            var encrypted = _protector.Protect(plainText);
            return EncryptedPrefix + encrypted;
        }

        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return encryptedText;

            // If not encrypted, return as-is (for backward compatibility)
            if (!IsEncrypted(encryptedText))
                return encryptedText;

            var withoutPrefix = encryptedText.Substring(EncryptedPrefix.Length);
            return _protector.Unprotect(withoutPrefix);
        }

        public bool IsEncrypted(string text)
        {
            return !string.IsNullOrEmpty(text) && text.StartsWith(EncryptedPrefix);
        }
    }
}
