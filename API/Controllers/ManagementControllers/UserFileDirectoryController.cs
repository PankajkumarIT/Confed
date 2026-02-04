using API.Data.IRepositories;
using API.Helpers;
using API.Helpers.Models;
using API.Model.ManagementModels;
using API.Model.ManagementModels.UserModels;
using API.Model.ViewModels;
using API.Model.ViewModels.TransporterManagementViewModels;
using API.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace API.Controllers.ManagementControllers
{
    [Route(SD.baseUrl+"fileDirectory")]
    [ApiController]
    [Authorize(Policy = SD.IsAccess)]
    public class UserFileDirectoryController : ControllerBase
    {
        private readonly IUnitofWork _unitofWork;
        private readonly IEncryptionHelper _encryptionHelper;
        public UserFileDirectoryController(IUnitofWork unitofWork,IEncryptionHelper encryptionHelper)
        {
            _unitofWork = unitofWork;
            _encryptionHelper = encryptionHelper;
        }
        [HttpPost("addDirectory")]
        public async Task<IActionResult> AddDirectory([FromBody] EncryptedDataVM details)
        {
            var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, details.Data);

            if (decryptedData != null)
            {
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                UserFileDirectory userFileDirectory = JsonSerializer.Deserialize<UserFileDirectory>(decryptedData);
                if (TryValidateModel(userFileDirectory))
                {
                    var alreadydriectoryname = await _unitofWork.UserFileDirectory.FirstOrDefaultAsync(x=>x.DirectoryName==userFileDirectory.DirectoryName && x.UserFileDirectoryCode==userFileDirectory.ParentDirectoryCode || x.DirectoryName==userFileDirectory.DirectoryName && x.ParentDirectoryCode ==null);

                    if (alreadydriectoryname == null)
                    {
                        UserFileDirectory userFileDirectory1 = new UserFileDirectory
                        {
                            UserFileDirectoryCode = _unitofWork.UserFileDirectory.GenrateUniqueCode(),
                            DirectoryName = userFileDirectory.DirectoryName,
                            ParentDirectoryCode = userFileDirectory.ParentDirectoryCode,
                            CreatedByUserCode = userInClaim.UserCode,
                            CreatedDate = DateTime.Now.ToLocalTime(),
                        };
                        await _unitofWork.UserFileDirectory.AddAsync(userFileDirectory1);
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, null)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                        return Ok(new { data = data.EncryptedData });
                    }
                    else
                    {
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, "Folder Name Allready Exists")));
                        HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                        return BadRequest(new { data = data.EncryptedData });
                    }
                }
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                   _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });
                }

            }
            else
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
               _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return BadRequest(new { data = data.EncryptedData });
            }
         

        }
        [HttpPost("renameDirectory")]
        public async Task<IActionResult> RenameDirectory([FromBody] EncryptedDataVM details)
        {
            var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, details.Data);

            if (decryptedData != null)
            {
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                UserFileDirectory userFileDirectory = JsonSerializer.Deserialize<UserFileDirectory>(decryptedData);
                if (TryValidateModel(userFileDirectory))
                {
                    var exists = await _unitofWork.UserFileDirectory.FirstOrDefaultAsync(x =>x.UserFileDirectoryCode == userFileDirectory.UserFileDirectoryCode);

                    if (exists != null)
                    {
                        await _unitofWork.UserFileDirectory.UpdateAsync(exists.UserFileDirectoryCode, async entity =>
                        {
                            entity.DirectoryName = userFileDirectory.DirectoryName;    
                           await Task.CompletedTask;
                        });
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                       _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Updated, null)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                        return Ok(new { data = data.EncryptedData });
                    }
                    else
                    {
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, "Folder Name Allready Exists")));
                        HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                        return BadRequest(new { data = data.EncryptedData });
                    }
                }
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                   _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });
                }
            }
            else
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return BadRequest(new { data = data.EncryptedData });
            }
        }
        [HttpPost("movedirectorywithfile")]
        public async Task<IActionResult> MoveDirectory([FromBody] EncryptedDataVM details)
        {
            var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, details.Data);

            if (decryptedData == null)
            {
                var fail = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                HttpContext.Response.Headers.Append("X-Data-Hash", fail.Hash);
                return BadRequest(new { data = fail.EncryptedData });
            }
            else
            {
                MoveDirectoryVM move = JsonSerializer.Deserialize<MoveDirectoryVM>(decryptedData);

                if (move.DirectoryCodes == null || move.DirectoryCodes.Count == 0)
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, "No folders selected")));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });
                }
                else
                {
                    var allDirs = await _unitofWork.UserFileDirectory.GetAllAsync(x => !x.IsDeleted);
                    var allFiles = await _unitofWork.UploadFileInfo.GetAllAsync(x => x.IsInternalOnly);
                    var lookup = allDirs.ToLookup(x => x.ParentDirectoryCode);

                    foreach (var dirCode in move.DirectoryCodes)
                    {
                        var sourceDir = allDirs.FirstOrDefault(x => x.UserFileDirectoryCode == dirCode);

                        if (sourceDir == null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                                _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, $"Source folder not found: {dirCode}")));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }
                        else
                        {
                            if (dirCode == move.NewDirectoryCode)
                            {
                                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                                    _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, "Folder cannot be moved into itself")));
                                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                                return BadRequest(new { data = data.EncryptedData });
                            }
                            else
                            {
                                var queueCheck = new Queue<string>();
                                queueCheck.Enqueue(dirCode);

                                bool invalidMove = false;

                                while (queueCheck.Count > 0)
                                {
                                    var current = queueCheck.Dequeue();
                                    foreach (var child in lookup[current])
                                    {
                                        if (child.UserFileDirectoryCode == move.NewDirectoryCode)
                                        {
                                            invalidMove = true;
                                            break;
                                        }
                                        queueCheck.Enqueue(child.UserFileDirectoryCode);
                                    }
                                    if (invalidMove) break;
                                }

                                if (invalidMove)
                                {
                                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, "Cannot move parent into child")));
                                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                                    return BadRequest(new { data = data.EncryptedData });
                                }
                                else
                                {
                                    var nameExists = allDirs.Any(x =>
                                        x.DirectoryName == sourceDir.DirectoryName &&
                                        x.ParentDirectoryCode == move.NewDirectoryCode &&
                                        x.UserFileDirectoryCode != dirCode);

                                    if (nameExists)
                                    {
                                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                                            _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, "Folder name already exists in target")));
                                        HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                                        return BadRequest(new { data = data.EncryptedData });
                                    }
                                    else
                                    {
                                        var folderTree = new List<string>();
                                        var q = new Queue<string>();
                                        q.Enqueue(dirCode);
                                        while (q.Count > 0)
                                        {
                                            var current = q.Dequeue();
                                            folderTree.Add(current);
                                            foreach (var child in lookup[current])
                                            q.Enqueue(child.UserFileDirectoryCode);
                                        }
                                        await _unitofWork.UserFileDirectory.UpdateAsync(dirCode, async entity =>
                                        {
                                            entity.ParentDirectoryCode = move.NewDirectoryCode;
                                            await Task.CompletedTask;
                                        });
                                        var filesToMove = allFiles.Where(f => f.UserFileDirectoryCode != null && folderTree.Contains(f.UserFileDirectoryCode)).ToList();

                                        foreach (var file in filesToMove)
                                        {
                                            await _unitofWork.UploadFileInfo.UpdateAsync(file.FileInfoCode, async entity =>
                                            {
                                                entity.UserFileDirectoryCode = move.NewDirectoryCode ?? dirCode ;
                                                await Task.CompletedTask;
                                            });
                                        }


                                    }
                                }
                            }
                        }
                    }

                    var success = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Updated, "Folders and files moved successfully")));
                    HttpContext.Response.Headers.Append("X-Data-Hash", success.Hash);
                    return Ok(new { data = success.EncryptedData });
                }
            }
        }
        [HttpPost("deleteDirectory")]
        public async Task<IActionResult> DeleteDirectory([FromBody] EncryptedDataVM details)
        {
            var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, details.Data);
            var deleteVM = JsonSerializer.Deserialize<StringValueVM>(decryptedData);
            var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

            var rootDir = await _unitofWork.UserFileDirectory.FirstOrDefaultAsync(d => d.UserFileDirectoryCode == deleteVM.Value && d.CreatedByUserCode == userInClaim.UserCode);
            bool deleted = await DeleteDirectoryRecursive(rootDir);  
            if(deleted==true)
            {
                var successData = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
             _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Deleted, null)));
                HttpContext.Response.Headers.Append("X-Data-Hash", successData.Hash);
                return Ok(new { data = successData.EncryptedData });
            }
            else
            {
                var successData = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
             _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, "Folder contains files")));
                HttpContext.Response.Headers.Append("X-Data-Hash", successData.Hash);
                return BadRequest(new { data = successData.EncryptedData });
            }
         
        }
        private async Task<bool> DeleteDirectoryRecursive(UserFileDirectory? dir)
        {
            if (dir == null || dir.IsDeleted)
                return false;

            var hasFilesHere = await _unitofWork.UploadFileInfo
                .FirstOrDefaultAsync(f => f.UserFileDirectoryCode == dir.UserFileDirectoryCode);

            if (hasFilesHere != null)
                return false;
            var subDirs = await _unitofWork.UserFileDirectory.GetAllAsync(d => d.ParentDirectoryCode == dir.UserFileDirectoryCode);
            foreach (var subDir in subDirs)
            {
                var canDeleteSub = await DeleteDirectoryRecursive(subDir);
               
                if (!canDeleteSub)
                    return false; 
            }
            await _unitofWork.UserFileDirectory.RemoveAsync(dir);

            return true;
        }

        //private async Task DeleteDirectoryRecursive(UserFileDirectory dir, string usercode)
        //{
        //     var files = await _unitofWork.UploadFileInfo
        //        .GetAllAsync(f => f.UserFileDirectoryCode == dir.UserFileDirectoryCode);
        //    object checkuser = "";

        //    var depuser = await _unitofWork.DepartmentUser.FirstOrDefaultAsync(x => x.UserCode == usercode, includeProperties: "Office");
        //    if (depuser != null)
        //    {
        //        checkuser = depuser;
        //    }
        //    else
        //    {
        //        var bankuser = await _unitofWork.DepartmentUser.FirstOrDefaultAsync(x => x.UserCode == usercode, includeProperties: "BankBranch");
        //        if(bankuser != null)
        //        {
        //            checkuser = bankuser;
        //        }
        //    }
        //    foreach (var file in files)
        //        {
        //            await _s3Helper.DeleteFileAsync(file.InternalFilePath);

        //            await _unitofWork.UploadFileInfo.RemoveAsync(file.FileInfoCode);
        //            await RemoveStorageAsync(file.FileSize, checkuser);
        //        }
        //    var subDirs = await _unitofWork.UserFileDirectory.GetAllAsync(d => d.ParentDirectoryCode == dir.UserFileDirectoryCode);
        //    foreach (var subDir in subDirs)
        //    {
        //        await DeleteDirectoryRecursive(subDir,usercode);
        //    }

        //    await _unitofWork.UserFileDirectory.RemoveAsync(dir.UserFileDirectoryCode);
        //}

        private async Task RemoveStorageAsync(double fileSize, object user)
        {
            try
            {
                if (user is DepartmentUser dept)
                {
                    await _unitofWork.DepartmentUser.UpdateAsync(dept.DepartmentUserCode, async entity =>
                    {
                        entity.UsedStorageSize -= fileSize;
                        if (entity.UsedStorageSize < 0) entity.UsedStorageSize = 0;
                        await Task.CompletedTask;
                    });

                    await _unitofWork.Office.UpdateAsync(dept.OfficeCode, async entity =>
                    {
                        entity.UsedStorageSize -= fileSize;
                        if (entity.UsedStorageSize < 0) entity.UsedStorageSize = 0;
                        await Task.CompletedTask;
                    });

                    await _unitofWork.Organization.UpdateAsync(dept.Office.OrganizationCode, async entity =>
                    {
                        entity.UsedStorageSize -= fileSize;
                        if (entity.UsedStorageSize < 0) entity.UsedStorageSize = 0;
                        await Task.CompletedTask;
                    });
                }
                else if (user is BankUser bankUser)
                {
                    await _unitofWork.BankUser.UpdateAsync(bankUser.BankUserCode, async entity =>
                    {
                        entity.UsedStorageSize -= fileSize;
                        if (entity.UsedStorageSize < 0) entity.UsedStorageSize = 0;
                        await Task.CompletedTask;
                    });

                    await _unitofWork.BankBranch.UpdateAsync(bankUser.BankBranchCode, async entity =>
                    {
                        entity.UsedStorageSize -= fileSize;
                        if (entity.UsedStorageSize < 0) entity.UsedStorageSize = 0;
                        await Task.CompletedTask;
                    });

                    await _unitofWork.Organization.UpdateAsync(bankUser.BankBranch.OrganizationCode, async entity =>
                    {
                        entity.UsedStorageSize -= fileSize;
                        if (entity.UsedStorageSize < 0) entity.UsedStorageSize = 0;
                        await Task.CompletedTask;
                    });
                }
            }
            catch (Exception)
            {

                throw;
            }

        }
    }
}
