

using API.Helpers.Models;

namespace API.Helpers
{
    public interface IEncryptionHelper
    { 
        public EncryptedDataAndHash Encrypt(string plainText);

        public string Decrypt(string encryptedText, string hash);

        public string ValidateDataHashAndData(IHeaderDictionary ApiHeaders, string encryptedText);

        public ReturnResponse GenrateResponse(bool status, StatusType statusTypeEnum, string message, dynamic data);

    }
}
