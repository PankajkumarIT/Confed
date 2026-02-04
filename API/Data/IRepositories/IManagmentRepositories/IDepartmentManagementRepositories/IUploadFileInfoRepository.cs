using API.Model.ManagementModels.BankManagement;
using API.Model.ManagementModels.DepartmentManagement;
using NPOI.SS.UserModel;
using System.Text;

namespace API.Data.IRepositories.IManagmentRepositories.IDepartmentManagementRepositories
{
    public interface IUploadFileInfoRepository:IRepository<UploadFileInfo>
    {

        (string fileHeader, List<string> rows,string filetype) ReadFile(IFormFile file);
        void ValidateHeaders(string uploadedHeader, string expectedHeader);
        void ValidateRequiredFieldConsistency(List<string> rows,List<string> expectedHeaders,BankBranch branch);
        void ConvertFile(string sourceFileContent,string targetHeaderCsv,Dictionary<string, string> columnMapping,out string convertedFileContent);
        string ConvertFileBytes(byte[] sourceFileBytes,string targetHeaderCsv,Dictionary<string, string> columnMapping);
        byte[] ProtectFileWithPassword(byte[] fileBytes,string password,string fileName);
        Task<(byte[] FileBytes, string FileName)> GetPasswordProtectedDownloadAsync(byte[] sourceFileBytes,string password,string targetHeaderCsv, string columnMappingString,string fileType);
        double GetFileStorage(byte[] fileBytes);
    }
}

