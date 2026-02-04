using API.Helpers.Models;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace API.Helpers
{
    public class EncryptionHelper : IEncryptionHelper
    {
        private readonly EncryptionSettings _settings;
        public EncryptionHelper(IOptions<EncryptionSettings> options)
        {
           _settings = options.Value;
        }
        public string ValidateDataHashAndData(IHeaderDictionary ApiHeaders, string encryptedText)
        {
            try
            {
                ApiHeaders.TryGetValue("X-Data-Hash", out var DataHash);

                if (DataHash.Count == 0)
                {
                    return null;
                }
                else
                {
                    DataHash = DataHash.ToString();

                    StringBuilder builder = new StringBuilder();

                    using (SHA256 sha256 = SHA256.Create())
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(encryptedText); // Convert string to bytes

                        var hashBytes = sha256.ComputeHash(bytes);

                        foreach (byte b in hashBytes)
                        {
                            builder.Append(b.ToString("x2"));
                        }
                    }
                    if (builder.ToString() != DataHash)
                    {
                        return null;
                    }
                    else
                    {
                        var data = Decrypt(encryptedText, DataHash);
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CryptographicException("An error occurred.", ex);
            }
        }

        public string Decrypt(string encryptedText, string hash)
        {
            if (string.IsNullOrEmpty(encryptedText))
                throw new ArgumentNullException(nameof(encryptedText), "Encrypted text cannot be null or empty.");

            try
            {
                    string decryptedText;

                    using (Aes aes = Aes.Create())
                    {
                        aes.Key = Encoding.UTF8.GetBytes(_settings.Key);
                        aes.IV = Encoding.UTF8.GetBytes(_settings.IV);

                        using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(encryptedText)))
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(
                                memoryStream,
                                aes.CreateDecryptor(),
                                CryptoStreamMode.Read))
                            {
                                using (StreamReader reader = new StreamReader(cryptoStream))
                                {
                                    decryptedText = reader.ReadToEnd();
                                }
                            }
                        }
                    }

                    return decryptedText;
            }
            catch (Exception ex)
            {
                throw new CryptographicException("An error occurred during decryption.", ex);
            }
        
        }
        public EncryptedDataAndHash Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText), "Plain text cannot be null or empty.");

            try
            {
                string encryptedText;
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(_settings.Key);
                    aes.IV = Encoding.UTF8.GetBytes(_settings.IV);


                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(
                            memoryStream,
                            aes.CreateEncryptor(),
                            CryptoStreamMode.Write))
                        {
                            using (StreamWriter writer = new StreamWriter(cryptoStream))
                            {
                                writer.Write(plainText);
                            }
                        }

                        encryptedText =  Convert.ToBase64String(memoryStream.ToArray());
                    }
                    StringBuilder builder = new StringBuilder();

                    using (SHA256 sha256 = SHA256.Create())
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(encryptedText); // Convert string to bytes

                        var hashBytes = sha256.ComputeHash(bytes);

                        foreach (byte b in hashBytes)
                        {
                            builder.Append(b.ToString("x2"));
                        }
                    }
                    EncryptedDataAndHash encryptedDataAndHash = new EncryptedDataAndHash()
                    {
                        EncryptedData = encryptedText,
                        Hash = builder.ToString()
                    };
                    return encryptedDataAndHash;

                }

            }
            catch (Exception ex)
            {
                throw new CryptographicException("An error occurred during encryption.", ex);
            }
        }

        public ReturnResponse GenrateResponse(bool status, StatusType statusTypeEnum, string message, dynamic data)
        {
            ReturnResponse returnResponse = new ReturnResponse()
            {
                message = message,
                statusTypeEnum = statusTypeEnum,
                data = data,
                status = status,
            };
            return returnResponse;
        }


        //public bool IsMatch(string encryptedText, string hash)
        //{
        //    try
        //    {
        //        string decryptedText = Decrypt(encryptedText, hash);

        //        //using (SHA256 sha256 = SHA256.Create())
        //        //{
        //        //    byte[] bytes = Encoding.UTF8.GetBytes(decryptedText); // Convert string to bytes

        //        //    var hashBytes = sha256.ComputeHash(bytes);

        //        //    StringBuilder builder = new StringBuilder();
        //        //    foreach (byte b in hashBytes)
        //        //    {
        //        //        builder.Append(b.ToString("x2"));
        //        //    }

        //        //    return builder.ToString() == hash;

        //        //}
        //    }
        //    catch
        //    {
        //        // Log exception if needed and return false
        //        return false;
        //    }
        //}

    }
}
