using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecurityToken.Model;
using Amazon.SecurityToken;
using API.Helpers.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Runtime;
using NPOI.HPSF;
using System.Text.RegularExpressions;
using Amazon.S3.Transfer;

public class S3Helper : IS3Helper
{
    private IAmazonS3 _s3;
    private readonly IConfiguration _config;
    private readonly S3ServiceModel _s3ServiceModel;

    private readonly TransferUtility _transferUtility;
    private DateTime _credentialsExpiration = DateTime.MinValue;
    public S3Helper(
          IAmazonS3 s3,
          IConfiguration config,
          IOptions<S3ServiceModel> s3ServiceModelOptions)
    {
        _s3 = s3; 
        _config = config;
        _s3ServiceModel = s3ServiceModelOptions.Value;
        _transferUtility = new TransferUtility(_s3);
    }

    private async Task<IAmazonS3> GetS3ClientAsync()
    {
        if (_s3 == null || (!string.IsNullOrEmpty(_s3ServiceModel.RoleArn) && DateTime.UtcNow.AddMinutes(5) >= _credentialsExpiration))
        {
            if (!string.IsNullOrEmpty(_s3ServiceModel.RoleArn))
            {
                var stsClient = new AmazonSecurityTokenServiceClient(RegionEndpoint.GetBySystemName(_s3ServiceModel.Region));
                var assumeRoleResponse = await stsClient.AssumeRoleAsync(new AssumeRoleRequest
                {
                    RoleArn = _s3ServiceModel.RoleArn,
                    RoleSessionName = "ct-sftp-backend-session",
                    DurationSeconds = 3600
                });

                var tempCreds = assumeRoleResponse.Credentials;
                _credentialsExpiration = (DateTime)tempCreds.Expiration;
                _s3 = new AmazonS3Client(
                   tempCreds.AccessKeyId,
                   tempCreds.SecretAccessKey,
                   tempCreds.SessionToken,
                   RegionEndpoint.GetBySystemName(_s3ServiceModel.Region)
               );
            }
            else
            {
                if (_s3 == null)
                    throw new InvalidOperationException("S3 client not initialized and cannot assume role.");
            }
        }

        return _s3;
    }
    public async Task<string> UploadtoS3(IFormFile file, string folderPath )
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");
        await GetS3ClientAsync();
        string bucketName = _s3ServiceModel.S3Bucket; 
        string timestamp = DateTime.Now.ToString("yyyyMMdd");

        string key = $"{folderPath}/{timestamp}_{file.FileName}";
        using (var stream = file.OpenReadStream())
        {
       
            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                InputStream = stream,
                ContentType = file.ContentType
            };
            await _s3.PutObjectAsync(request);
        }

        return key;
    }
    public async Task<string> CopyFileAsync(string sourceKey, string destinationFolder)
    {
        await GetS3ClientAsync();

        string bucketName = _s3ServiceModel.S3Bucket;
        string fileName = Path.GetFileName(sourceKey);
        string destinationKey = $"{destinationFolder}/{fileName}";

        await _s3.CopyObjectAsync(new CopyObjectRequest
        {
            SourceBucket = bucketName,
            SourceKey = sourceKey,
            DestinationBucket = bucketName,
            DestinationKey = destinationKey
        });
        return destinationKey;
    }

    public async Task<string> MoveFileAsync(string sourceKey, string destinationFolder)
    {
       await GetS3ClientAsync();

        string bucketName = _s3ServiceModel.S3Bucket;
        string fileName = Path.GetFileName(sourceKey);
        string destinationKey = $"{destinationFolder}/{fileName}";
        await _s3.CopyObjectAsync(new CopyObjectRequest
        {
            SourceBucket = bucketName,
            SourceKey = sourceKey,
            DestinationBucket = bucketName,
            DestinationKey = destinationKey
        });

        await _s3.DeleteObjectAsync(bucketName, sourceKey);

        return destinationKey;
    }

    public async Task<(byte[] fileBytes, string contentType, string fileName)> GetFileAsync(string key)
    {
        await GetS3ClientAsync();
        string bucketName = _s3ServiceModel.S3Bucket;

        var request = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = key
        };

        using var response = await _s3.GetObjectAsync(request);
        using var memoryStream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memoryStream);

        var storedFileName = Path.GetFileName(key);

        string originalFileName = storedFileName;
        if (storedFileName.Length > 9 && storedFileName[8] == '_')
        {
            originalFileName = storedFileName.Substring(9);
        }

        return (
            memoryStream.ToArray(),
            response.Headers.ContentType ?? "application/octet-stream",
            originalFileName
        );
    }


    //public async Task<List<string>> GetAllFilesAsync(string path = "")
    //{
    //    var s3 = await GetS3ClientAsync();  
    //    var request = new ListObjectsV2Request
    //    {
    //        BucketName = _s3ServiceModel.S3Bucket,  
    //        Prefix = path
    //    };

    //    var response = await s3.ListObjectsV2Async(request);

    //    return response.S3Objects
    //        .Select(x => x.Key)
    //        .ToList();
    //}
    public async Task DeleteFileAsync(string s3Key)
    {
        await GetS3ClientAsync();

        await _s3.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = _s3ServiceModel.S3Bucket,
            Key = s3Key
        });
    }

}
