using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace PersonalFinanceTracker.Model.Helpers
{
    public class EncryptionHelper
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        // Constructor to initialize the key and IV (can be passed in or auto-generated)
        public EncryptionHelper(IConfiguration configuration)
        {
            var encryptionSettings = configuration.GetSection("EncryptionSettings");

            // Convert the Key and IV from Base64 or plain text to byte arrays
            _key = Encoding.UTF8.GetBytes(encryptionSettings.GetSection("Key").Value);
            _iv = Encoding.UTF8.GetBytes(encryptionSettings.GetSection("IV").Value);
        }

        // Generic method to encrypt a string
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = _key;
                aesAlg.IV = _iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }

                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        // Generic method to decrypt a string
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return null;

            if (!IsBase64String(cipherText))
            {
                // If not a valid Base64 string, assume it's plain text and return it as is
                return cipherText;
            }

            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = _key;
                    aesAlg.IV = _iv;

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                try
                                {
                                    return srDecrypt.ReadToEnd();
                                }
                                catch (Exception ex)
                                {
                                    return cipherText;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return cipherText;
            }
        }

        public static bool IsBase64String(string base64)
        {
            if (string.IsNullOrEmpty(base64) || base64.Length % 4 != 0)
                return false;

            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out _);
        }
    }
}
