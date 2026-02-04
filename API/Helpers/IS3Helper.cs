using Amazon.S3;

public interface IS3Helper
{
    Task<string> UploadtoS3(IFormFile file, string folderPath);
    Task<string> CopyFileAsync(string sourceKey, string destinationFolder);
    Task<string> MoveFileAsync(string sourceKey, string destinationFolder);
    Task DeleteFileAsync(string s3Key);
    Task<(byte[] fileBytes, string contentType, string fileName)> GetFileAsync(string key);
 //   Task<List<string>> GetAllFilesAsync(string path = "");

}
