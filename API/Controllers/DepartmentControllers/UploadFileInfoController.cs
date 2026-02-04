using Amazon.Runtime.Internal;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecurityToken;
using API.Data.IRepositories;
using API.Data.Repositories;
using API.Helpers;
using API.Helpers.Models;
using API.Model.ManagementModels;
using API.Model.ManagementModels.DepartmentManagement;
using API.Model.ManagementModels.UserModels;
using API.Model.QueryParamViewModels;
using API.Model.UserModels;
using API.Model.ViewModels;
using API.Model.ViewModels.TransporterManagementViewModels;
using API.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPOI.OpenXml4Net.OPC.Internal;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using Org.BouncyCastle.Asn1.Ocsp;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace API.Controllers.DepartmentControllers
{
    [Route(SD.baseUrl + "UploadFileInfo")]
    [ApiController]
     [Authorize(Policy = SD.IsAccess)]

    public class UploadFileInfoController : ControllerBase
    {
        private readonly IAmazonS3 _s3;
        private readonly IUnitofWork _unitofWork;
        private readonly IEncryptionHelper _encryptionHelper;
        private readonly IS3Helper _s3Helper;
        public UploadFileInfoController(IUnitofWork unitofWork, IAmazonS3 s3, IEncryptionHelper encryptionHelper, IS3Helper s3Helper)
        {
            _encryptionHelper = encryptionHelper;
            _unitofWork = unitofWork;
            _s3Helper = s3Helper;

            _s3 = s3;
        }

        [HttpPost("uploadFile")]
        public async Task<IActionResult> UploadFile([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);
                if (decryptedData != null)
                {
                    var user = JsonSerializer.Deserialize<UploadFileInfoVM>(decryptedData);
                    if (TryValidateModel(user))
                    {
                        var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                        var OfficeInClaim = await _unitofWork.Office.FirstOrDefaultAsync(x => x.OfficeCode == User.FindFirst(SD.EntityCode).Value);
                        var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                        var org = await _unitofWork.DepartmentBankOrganizationsMap.FirstOrDefaultAsync(x => x.DepartmentOrganizationCode == user.OrganizationCode);
                        var file = _unitofWork.Organization.DecrypteIFromFileWithFileNameBase64(user.UploadFile, user.FileName);
                        byte[] fileBytes;
                        using (var ms = new MemoryStream())
                        {
                            await file.CopyToAsync(ms);
                            fileBytes = ms.ToArray();
                        }
                        double fileSize = _unitofWork.UploadFileInfo.GetFileStorage(fileBytes);
                        var (fileHeader, rows, filetype) = _unitofWork.UploadFileInfo.ReadFile(file);
                        object Getuser = null;
                        var departmentUser = await _unitofWork.DepartmentUser.FirstOrDefaultAsync(x => x.UserCode == userInClaim.UserCode);
                        if (departmentUser != null)
                        {
                            Getuser = departmentUser;

                            double storageleft = departmentUser.UsedStorageSize + fileSize;
                            if (departmentUser.TotalStorageSize < storageleft)

                            {
                                var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.failure, ResponseHandler.NoStorage, null)));
                                HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                                return BadRequest(new { data = okdata.EncryptedData });
                            }

                        }
                        else
                        {
                            var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.failure, ResponseHandler.NotFound, " Office User Is Not Found")));
                            HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                            return NotFound(new { data = okdata.EncryptedData });
                        }
                        if (file == null)
                        {
                            var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.failure, ResponseHandler.NotFound, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                            return NotFound(new { data = okdata.EncryptedData });
                        }
                        if (user.IsSftpOnly == true)
                        {

                            string orgFolder = $"org-{OfficeInClaim.OrganizationCode}";
                            string officeFolder = $"office-{OfficeInClaim.OfficeCode}";
                            string userFolder = $"user-{userInClaim.UserCode}";
                            string privacyFolder = "files";
                            string folderPath = $"{orgFolder}/{officeFolder}/{userFolder}/{privacyFolder}";
                            var s3Key = await _s3Helper.UploadtoS3(file, folderPath);
                            var singleRecord = new UploadFileInfo
                            {
                                FileInfoCode = _unitofWork.UploadFileInfo.GenrateUniqueCode(),
                                FileNumber = await _unitofWork.UploadFileInfo.GenerateFileNumberAsync(),
                                FileName = user.FileName,
                                OrganizationCode = user.OrganizationCode,
                                FileType = filetype,
                                DepartmentApprovalStatus = "Approved",
                                Status = "InternalOnly",
                                RequestedDate = DateTime.Now.ToLocalTime(),
                                OfficeCode = OfficeInClaim?.OfficeCode,
                                OfficeName = OfficeInClaim?.OfficeName,
                                UserCode = User.FindFirstValue(ClaimTypes.SerialNumber),
                                IsInternalOnly = user.IsSftpOnly,
                                FileSize = fileSize,
                                TotalCount = rows.Count,
                                 UserFileDirectoryCode=user.UserFileDirectoryCode,
                                InternalFilePath = s3Key,
                                IsPrivate = true,
                            };
                            await _unitofWork.UploadFileInfo.AddAsync(singleRecord);
                            await AddStorageAsync(fileSize, Getuser);

                            var ok = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Created, singleRecord)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", ok.Hash);
                            return Ok(new { data = ok.EncryptedData });
                        }
                        else if (user.IsDraft == true)
                        {
                            _unitofWork.UploadFileInfo.ValidateHeaders(fileHeader, org.InputFileHeader);
                            var branch = await _unitofWork.BankBranch.FirstOrDefaultAsync(x => x.BankBranchCode == user.BankBranchCode);
                            _unitofWork.UploadFileInfo.ValidateRequiredFieldConsistency(rows, org.InputFileHeader.Split(',').ToList(), branch);
                            string orgFolder = $"org-{OfficeInClaim.OrganizationCode}";
                            string officeFolder = $"office-{OfficeInClaim.OfficeCode}";
                            string bankFolder = $"institute-{branch.BankBranchCode}";
                            string oubondFolder = "outbound";

                            string privacyfolder = "draft";
                            string folderPath = $"{orgFolder}/{officeFolder}/{bankFolder}/{oubondFolder}/{privacyfolder}";
                            var s3Key = await _s3Helper.UploadtoS3(file, folderPath);
                            var singleRecord = new UploadFileInfo
                            {
                                FileInfoCode = _unitofWork.UploadFileInfo.GenrateUniqueCode(),
                                FileNumber = await _unitofWork.UploadFileInfo.GenerateFileNumberAsync(),
                                OfficeCode = OfficeInClaim?.OfficeCode,
                                OfficeName = OfficeInClaim.OfficeName,
                                FileName = user.FileName,
                                OrganizationCode = user.OrganizationCode,
                                FileType = filetype,
                                DepartmentApprovalStatus = "Pending",
                                BankBranchCode = user.BankBranchCode,
                                FileSize = fileSize,
                                BankName = branch.BankName,
                                BranchName = branch.BranchName,
                                IFSC = branch.IFSC,
                                Status = "Draft",
                                RequestedDate = DateTime.Now.ToLocalTime(),
                                RequestFilePath = s3Key,
                                UserCode = User.FindFirstValue(ClaimTypes.SerialNumber),
                                ResponseOrganizationCode = user.BankOrganizationCode,
                                IsInternalOnly = user.IsSftpOnly,
                                TotalCount = rows.Count,
                            };
                            await _unitofWork.UploadFileInfo.AddAsync(singleRecord);
                            UploadFileInfoHistory fileHistory = new UploadFileInfoHistory
                            {
                                UploadFileInfoHistoryCode = _unitofWork.UploadFileInfoHistory.GenrateUniqueCode(),
                                FileInfoCode = singleRecord.FileInfoCode,
                                ActionByUserCode = userInClaim.UserCode,
                                ActionByRoleCode = userRoleInClaim.RoleCode,
                                AssignedToRoleCode = user.AssignedToRoleCode,
                                AssignedToUserCode = user.AssignedToUserCode,
                                ActionDate = DateTime.Now.ToLocalTime(),
                                Status = "Forwarded"
                            };
                            await _unitofWork.UploadFileInfoHistory.AddAsync(fileHistory);
                            await AddStorageAsync(fileSize, Getuser);
                            var ok = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Created, singleRecord)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", ok.Hash);
                            return Ok(new { data = ok.EncryptedData });
                        }
                        else
                        {
                            _unitofWork.UploadFileInfo.ValidateHeaders(fileHeader, org.InputFileHeader);
                            var branch = await _unitofWork.BankBranch.FirstOrDefaultAsync(x => x.BankBranchCode == user.BankBranchCode);
                            _unitofWork.UploadFileInfo.ValidateRequiredFieldConsistency(rows, org.InputFileHeader.Split(',').ToList(), branch);
                            string orgFolder = $"org-{OfficeInClaim.OrganizationCode}";
                            string officeFolder = $"office-{OfficeInClaim.OfficeCode}";
                            string bankFolder = $"institute-{branch.BankBranchCode}";
                            string oubondFolder = "outbound";

                            string privacyfolder = "outbox";
                            string folderPath = $"{orgFolder}/{officeFolder}/{bankFolder}/{oubondFolder}/{privacyfolder}";
                            var s3Key = await _s3Helper.UploadtoS3(file, folderPath);
                            var singleRecord = new UploadFileInfo
                            {
                                FileInfoCode = _unitofWork.UploadFileInfo.GenrateUniqueCode(),
                                FileNumber = await _unitofWork.UploadFileInfo.GenerateFileNumberAsync(),
                                OfficeCode = OfficeInClaim?.OfficeCode,
                                OfficeName = OfficeInClaim.OfficeName,
                                FileName = user.FileName,
                                OrganizationCode = user.OrganizationCode,
                                FileType = filetype,
                                DepartmentApprovalStatus = "Pending",
                                BankBranchCode = user.BankBranchCode,
                                FileSize = fileSize,
                                BankName = branch.BankName,
                                BranchName = branch.BranchName,
                                IFSC = branch.IFSC,
                                Status = "Requested",
                                RequestedDate = DateTime.Now.ToLocalTime(),
                                RequestFilePath = s3Key,
                                UserCode = User.FindFirstValue(ClaimTypes.SerialNumber),
                                ResponseOrganizationCode = user.BankOrganizationCode,
                                IsInternalOnly = user.IsSftpOnly,
                                TotalCount = rows.Count,
                            };
                            await _unitofWork.UploadFileInfo.AddAsync(singleRecord);
                            if (user.IsSftpOnly == false)
                            {
                                UploadFileInfoHistory fileHistory = new UploadFileInfoHistory
                                {
                                    UploadFileInfoHistoryCode = _unitofWork.UploadFileInfoHistory.GenrateUniqueCode(),
                                    FileInfoCode = singleRecord.FileInfoCode,
                                    ActionByUserCode = userInClaim.UserCode,
                                    ActionByRoleCode = userRoleInClaim.RoleCode,
                                    AssignedToRoleCode = user.AssignedToRoleCode,
                                    AssignedToUserCode = user.AssignedToUserCode,
                                    ActionDate = DateTime.Now.ToLocalTime(),
                                    Status = "Forwarded"
                                };
                                await _unitofWork.UploadFileInfoHistory.AddAsync(fileHistory);
                            }
                            await AddStorageAsync(fileSize, Getuser);
                            var ok = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Created, singleRecord)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", ok.Hash);
                            return Ok(new { data = ok.EncryptedData });

                        }
                    }
                    else
                    {
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList())));
                        HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                        return BadRequest(new { data = data.EncryptedData });
                    }
                }
                else
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });
                }
            }
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new { data = data.EncryptedData });
            }
        }
        [HttpPost("sharedFile")]
        public async Task<IActionResult> FileShared([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);
                if (decryptedData != null)
                {
                    SharedUser user = JsonSerializer.Deserialize<SharedUser>(decryptedData);
                    if (user.UserCode != null)
                    {
                        var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                        var OfficeInClaim = await _unitofWork.Office.FirstOrDefaultAsync(x => x.OfficeCode == User.FindFirst(SD.EntityCode).Value);

                        var fileindb = await _unitofWork.UploadFileInfo.FirstOrDefaultAsync(x => x.FileInfoCode == user.FileInfoCode && x.UserCode == userInClaim.UserCode);
                        if (fileindb == null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, "Not Access Of shared this file")));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }
                        else
                        {


                            await _unitofWork.UploadFileInfo.UpdateAsync(fileindb.FileInfoCode, async entity =>
                            {
                                entity.SharedUsers = System.Text.Json.JsonSerializer.Serialize(user);
                                entity.IsPrivate = true;
                                await Task.CompletedTask;
                            });
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Updated, "File Shared Successfully")));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return Ok(new { data = data.EncryptedData });
                        }
                    }
                    else
                    {
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                        return NotFound(new { data = data.EncryptedData });
                    }

                }
                else
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });
                }

            }
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new { data = data.EncryptedData });
            }
        }
        [HttpPost("uploadResponseFileByBank")]
        public async Task<IActionResult> UploadResponseByBank([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);
                if (decryptedData != null)
                {

                    var user = JsonSerializer.Deserialize<BankUploadFileVM>(decryptedData);
                    if (TryValidateModel(user))
                    {
                        var org = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == User.FindFirstValue(SD.OrganizationCode));
                        var getfiledata = await _unitofWork.UploadFileInfo.FirstOrDefaultAsync(x => x.FileInfoCode == user.FileInfoCode);

                        var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                        var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                        string responsefilepath = "";
                        string nackfilepath = "";
                        string ackfilepath = "";
                        string bankresponsefilepath = "";
                        string banknackfilepath = "";
                        string bankackfilepath = "";

                        if (user.ResponseFile != null)
                        {
                            var Responsefile = _unitofWork.Organization.DecrypteIFromFileWithFileNameBase64(user.ResponseFile, user.ResponseFileName);
                            var (responseHeader, responseRows, responseType) = _unitofWork.UploadFileInfo.ReadFile(Responsefile);
                            _unitofWork.UploadFileInfo.ValidateHeaders(responseHeader, org.PaymentResponseFileHeader);
                            string orgFolder = $"org-{getfiledata.ResponseOrganizationCode}";
                            string officeFolder = $"office-{getfiledata.BankBranchCode}";
                            string bankFolder = $"institute-{getfiledata.OfficeCode}";
                            string outboundFolder = "outbound";
                            string privacyFolder = "sent";
                            string folderPath = $"{orgFolder}/{officeFolder}/{bankFolder}/{outboundFolder}/{privacyFolder}";
                            bankresponsefilepath = await _s3Helper.UploadtoS3(Responsefile, folderPath);
                            if (bankresponsefilepath != null)
                            {

                                string targetOrgFolder = $"org-{getfiledata.OrganizationCode}";
                                string targetOfficeFolder = $"office-{getfiledata.OfficeCode}";
                                string targetBankFolder = $"institute-{getfiledata.BankBranchCode}";
                                string inboundFolder = "inbound";
                                string inboundFolderPath = $"{targetOrgFolder}/{targetOfficeFolder}/{targetBankFolder}/{inboundFolder}";
                                responsefilepath = await _s3Helper.CopyFileAsync(bankresponsefilepath, inboundFolderPath);

                            }
                        }
                        if (user.AcknowledgmentFile != null)
                        {
                            var AcknowledgmentFile = _unitofWork.Organization.DecrypteIFromFileWithFileNameBase64(user.AcknowledgmentFile, user.AcknowledgmentFileName);
                            var (ackHeader, ackRows, ackType) = _unitofWork.UploadFileInfo.ReadFile(AcknowledgmentFile);
                            _unitofWork.UploadFileInfo.ValidateHeaders(ackHeader, org.PaymentAcknowledgment);
                            string orgFolder = $"org-{getfiledata.ResponseOrganizationCode}";
                            string officeFolder = $"office-{getfiledata.BankBranchCode}";
                            string bankFolder = $"institute-{getfiledata.OfficeCode}";
                            string outboundFolder = "outbound";
                            string privacyFolder = "sent";
                            string folderPath = $"{orgFolder}/{officeFolder}/{bankFolder}/{outboundFolder}/{privacyFolder}";
                            bankackfilepath = await _s3Helper.UploadtoS3(AcknowledgmentFile, folderPath);
                            if (bankresponsefilepath != null)
                            {

                                string targetOrgFolder = $"org-{getfiledata.OrganizationCode}";
                                string targetOfficeFolder = $"office-{getfiledata.OfficeCode}";
                                string targetBankFolder = $"institute-{getfiledata.BankBranchCode}";
                                string inboundFolder = "inbound";
                                string inboundFolderPath = $"{targetOrgFolder}/{targetOfficeFolder}/{targetBankFolder}/{inboundFolder}";
                                ackfilepath = await _s3Helper.CopyFileAsync(bankackfilepath, inboundFolderPath);

                            }


                        }
                        else
                        {
                            var NoAcknowledgmentFile = _unitofWork.Organization.DecrypteIFromFileWithFileNameBase64(user.NoAcknowledgmentFile, user.NoAcknowledgmentFileName);
                            var (noackHeader, noackRows, noackType) = _unitofWork.UploadFileInfo.ReadFile(NoAcknowledgmentFile);
                            _unitofWork.UploadFileInfo.ValidateHeaders(noackHeader, org.PaymentNotAcknowledgement);
                            string orgFolder = $"org-{getfiledata.ResponseOrganizationCode}";
                            string officeFolder = $"office-{getfiledata.BankBranchCode}";
                            string bankFolder = $"institute-{getfiledata.OfficeCode}";
                            string outboundFolder = "outbound";
                            string privacyFolder = "sent";
                            string folderPath = $"{orgFolder}/{officeFolder}/{bankFolder}/{outboundFolder}/{privacyFolder}";
                            banknackfilepath = await _s3Helper.UploadtoS3(NoAcknowledgmentFile, folderPath);
                            if (bankresponsefilepath != null)
                            {

                                string targetOrgFolder = $"org-{getfiledata.OrganizationCode}";
                                string targetOfficeFolder = $"office-{getfiledata.OfficeCode}";
                                string targetBankFolder = $"institute-{getfiledata.BankBranchCode}";
                                string inboundFolder = "inbound";
                                string inboundFolderPath = $"{targetOrgFolder}/{targetOfficeFolder}/{targetBankFolder}/{inboundFolder}";
                                nackfilepath = await _s3Helper.CopyFileAsync(banknackfilepath, inboundFolderPath);

                            }
                        }
                        if (getfiledata != null)
                        {
                            await _unitofWork.UploadFileInfo.UpdateAsync(getfiledata.FileInfoCode, async entity =>
                            {
                                entity.AcknowledgementFileName = user.AcknowledgmentFileName;
                                entity.NoAcknowledgementFileName = user.NoAcknowledgmentFileName;
                                entity.ResponseFileName = user.ResponseFileName;
                                entity.AcknowledgementFileNamePath = ackfilepath;
                                entity.NoAcknowledgementFileNamePath = nackfilepath;
                                entity.ResponseFilePath = responsefilepath;
                                entity.BankAcknowledgementFilePath = bankackfilepath;
                                entity.BankNoAcknowledgementFilePath = banknackfilepath;
                                entity.BankResponsePath = bankresponsefilepath;
                                entity.ResponseUserCode = userInClaim.UserCode;
                                entity.Status = "Response";
                                entity.ResponseDate = DateTime.Now.ToLocalTime();
                                await Task.CompletedTask;
                            });
                        }

                        var ok = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Created, null)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", ok.Hash);
                        return Ok(new { data = ok.EncryptedData });
                    }
                    else
                    {
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList())));
                        HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                        return BadRequest(new { data = data.EncryptedData });
                    }

                }
                else
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                       _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)
                   ));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });
                }
            }
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)
                ));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new { data = data.EncryptedData });
            }
        }
        [HttpPost("getFileByKey")]
        public async Task<IActionResult> GetFileByKey([FromBody] EncryptedDataVM Details)
        {
            try
            {

                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);
                var user = JsonSerializer.Deserialize<StringValueVM>(decryptedData);
                string s3Key = user.Value;
                var (fileBytes, contentType, fileName) = await _s3Helper.GetFileAsync(s3Key);
                string base64File = Convert.ToBase64String(fileBytes);
                var encrypted = _encryptionHelper.Encrypt(JsonSerializer.Serialize(new
                {
                    FileName = fileName,
                    ContentType = contentType,
                    FileBase64 = base64File
                }));
                HttpContext.Response.Headers.Append("X-Data-Hash", encrypted.Hash);
                return Ok(new { data = encrypted.EncryptedData });
            }
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new { data = data.EncryptedData });
            }
        }

        [HttpPost("fileRejetedByDepartmentUser")]
        public async Task<IActionResult> FileRejetedByDepartmentUser([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);
                if (decryptedData != null)
                {
                    FileApprovalVM requestData = JsonSerializer.Deserialize<FileApprovalVM>(decryptedData);
                    if (TryValidateModel(requestData))
                    {
                        var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                        var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                        var uploadfileindb = await _unitofWork.UploadFileInfoHistory.FirstOrDefaultAsync(x => x.FileInfoCode == requestData.FileInfoCode, includeProperties: "UploadFileInfo");
                        if (uploadfileindb != null)
                        {
                            var getallroleindb = await _unitofWork.UserRole.GetAllAsync(x => x.RoleType == "department");
                            if ((userRoleInClaim.RoleLevel == RoleLevels.INTERMEDIATE && userRoleInClaim.RoleType == "department") ||
                                (userRoleInClaim.RoleLevel == RoleLevels.AUTHORITY && userRoleInClaim.RoleType == "department")
                                )
                            {


                                string orgFolder = $"org-{uploadfileindb.UploadFileInfo.OrganizationCode}";
                                string officeFolder = $"office-{uploadfileindb.UploadFileInfo.OfficeCode}";
                                string bankFolder = $"institute-{uploadfileindb.UploadFileInfo.BankBranchCode}";
                                string oubondFolder = "outbound";

                                string privacyfolder = "rejected";

                                string sentFolderPath = $"{orgFolder}/{officeFolder}/{bankFolder}/{oubondFolder}/{privacyfolder}";
                                var copyFilePath = await _s3Helper.MoveFileAsync(uploadfileindb.UploadFileInfo.RequestFilePath, sentFolderPath);

                                UploadFileInfoHistory routeApprovalHistory = new UploadFileInfoHistory()
                                {
                                    UploadFileInfoHistoryCode = _unitofWork.UploadFileInfoHistory.GenrateUniqueCode(),
                                    FileInfoCode = requestData.FileInfoCode,
                                    ActionByUserCode = userInClaim.UserCode,
                                    ActionByRoleCode = userRoleInClaim.RoleCode,
                                    ActionDate = DateTime.Now.ToLocalTime(),
                                    Status = "Rejected"
                                };
                                await _unitofWork.UploadFileInfoHistory.AddAsync(routeApprovalHistory);

                                await _unitofWork.UploadFileInfo.UpdateAsync(uploadfileindb.FileInfoCode, async entity =>
                                {
                                    entity.DepartmentApprovalStatus = "Rejected";
                                    entity.RejectFilePath = copyFilePath;
                                    entity.Status = "Rejected";
                                    await Task.CompletedTask;
                                });
                                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                               (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Updated, null)));
                                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                                return Ok(new { data = data.EncryptedData });
                            }
                            else
                            {
                                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NoAccess, null)));
                                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                                return BadRequest(new { data = data.EncryptedData });
                            }
                        }
                        else
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                        (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return NotFound(new { data = data.EncryptedData });
                        }

                    }
                    else
                    {
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                        (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                        return BadRequest(new { data = data.EncryptedData });
                    }
                }
                else
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                            (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });

                }
            }
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                               (_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new
                {
                    data = data.EncryptedData
                });
            }
        }
        [HttpPost("fileApprovalForDepartment")]
        public async Task<IActionResult> FileApprovalForDepartment([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);
                if (decryptedData != null)
                {
                    FileApprovalVM requestData = JsonSerializer.Deserialize<FileApprovalVM>(decryptedData);
                    if (TryValidateModel(requestData))
                    {
                        var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                        var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                        var OfficeInClaim = await _unitofWork.Office.FirstOrDefaultAsync(x => x.OfficeCode == User.FindFirst(SD.EntityCode).Value);

                        var uploadfileindb = await _unitofWork.UploadFileInfoHistory.FirstOrDefaultAsync(x => x.FileInfoCode == requestData.FileInfoCode, includeProperties: "UploadFileInfo");
                        var branch = await _unitofWork.BankBranch.FirstOrDefaultAsync(x => x.BankBranchCode == uploadfileindb.UploadFileInfo.BankBranchCode);

                        if (uploadfileindb != null)
                        {
                            var getallroleindb = await _unitofWork.UserRole.GetAllAsync(x => x.RoleType == "department");
                            if ((userRoleInClaim.RoleLevel == RoleLevels.INTERMEDIATE && userRoleInClaim.RoleType == "department") ||
                                (userRoleInClaim.RoleLevel == RoleLevels.AUTHORITY && userRoleInClaim.RoleType == "department")
                                )
                            {
                                string orgFolder = $"org-{uploadfileindb.UploadFileInfo.OrganizationCode}";
                                string officeFolder = $"office-{uploadfileindb.UploadFileInfo.OfficeCode}";
                                string bankFolder = $"institute-{branch.BankBranchCode}";
                                string oubondFolder = "outbound";

                                string privacyfolder = "sent";

                                string sentFolderPath = $"{orgFolder}/{officeFolder}/{bankFolder}/{oubondFolder}/{privacyfolder}";
                                var copyFilePath = await _s3Helper.MoveFileAsync(uploadfileindb.UploadFileInfo.RequestFilePath, sentFolderPath);
                                string movedS3Key = "";
                                if (copyFilePath != null)
                                {
                                    string targetOrgFolder = $"org-{uploadfileindb.UploadFileInfo.ResponseOrganizationCode}";
                                    string targetOfficeFolder = $"office-{uploadfileindb.UploadFileInfo.BankBranchCode}";
                                    string targetBankFolder = $"institute-{uploadfileindb.UploadFileInfo.OfficeCode}";
                                    string inboundFolder = "inbound";
                                    string inboundFolderPath = $"{targetOrgFolder}/{targetOfficeFolder}/{targetBankFolder}/{inboundFolder}";
                                    movedS3Key = await _s3Helper.CopyFileAsync(copyFilePath, inboundFolderPath);
                                }
                                UploadFileInfoHistory routeApprovalHistory = new UploadFileInfoHistory()
                                {
                                    UploadFileInfoHistoryCode = _unitofWork.UploadFileInfoHistory.GenrateUniqueCode(),
                                    FileInfoCode = requestData.FileInfoCode,
                                    ActionByUserCode = userInClaim.UserCode,
                                    ActionByRoleCode = userRoleInClaim.RoleCode,
                                    ActionDate = DateTime.Now.ToLocalTime(),
                                    Status = "Approved"
                                };
                                await _unitofWork.UploadFileInfoHistory.AddAsync(routeApprovalHistory);
                                await _unitofWork.UploadFileInfo.UpdateAsync(uploadfileindb.FileInfoCode, async entity =>
                                {
                                    entity.DepartmentApprovalStatus = "Approved";
                                    entity.InProcessFilePath = copyFilePath;
                                    entity.RequestFilePath = null;
                                    entity.BankProcessFilePath = movedS3Key;
                                    entity.InprocessDate = DateTime.Now.ToLocalTime();
                                    entity.Status = "InProcess";
                                    await Task.CompletedTask;
                                });
                                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                               (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Updated, null)));
                                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                                return Ok(new { data = data.EncryptedData });
                            }
                            else
                            {

                                UploadFileInfoHistory routeApprovalHistory = new UploadFileInfoHistory()
                                {
                                    UploadFileInfoHistoryCode = _unitofWork.UploadFileInfoHistory.GenrateUniqueCode(),
                                    FileInfoCode = requestData.FileInfoCode,
                                    ActionByUserCode = userInClaim.UserCode,
                                    ActionByRoleCode = userRoleInClaim.RoleCode,
                                    AssignedToRoleCode = requestData.AssignedToRoleCode,
                                    AssignedToUserCode = requestData.AssignedToUserCode,
                                    ActionDate = DateTime.Now.ToLocalTime(),
                                    Status = "Forwarded"
                                };
                                await _unitofWork.UploadFileInfoHistory.AddAsync(routeApprovalHistory);
                                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                               (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Updated, null)));
                                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                                return Ok(new { data = data.EncryptedData });
                            }
                        }
                        else
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                        (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return NotFound(new { data = data.EncryptedData });
                        }

                    }
                    else
                    {
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                        (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                        return BadRequest(new { data = data.EncryptedData });
                    }
                }
                else
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                            (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });

                }
            }
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                               (_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new
                {
                    data = data.EncryptedData
                });
            }
        }
        [HttpGet("getAllFileByBankUser")]
        public async Task<IActionResult> GetAllFileByBankUser()
        {
            try
            {
                var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(
                 x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                List<UploadFileInfo> uploadFile = new List<UploadFileInfo>();
                if (userInClaim.IsEntityUser)
                {
                    var organizationClaim = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == User.FindFirst(SD.OrganizationCode).Value);
                    var adminuser = await _unitofWork.AdministorUser.FirstOrDefaultAsync(
                        x => x.UserCode == userInClaim.UserCode);
                    if (adminuser != null)
                    {
                        uploadFile = (await _unitofWork.UploadFileInfo.GetAllAsync(x => x.BankName == organizationClaim.OrganizationName && x.ResponseOrganizationCode == organizationClaim.OrganizationCode && x.Status == "Response", includeProperties: "Organization,User")).ToList();

                    }
                    else
                    {
                        var department = await _unitofWork.BankUser.FirstOrDefaultAsync(x => x.BankUserCode == User.FindFirst(SD.UserTypeCode).Value, includeProperties: "BankBranch");

                        if (department == null)
                        {
                            var err = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));

                            HttpContext.Response.Headers.Append("X-Data-Hash", err.Hash);
                            return NotFound(new { data = err.EncryptedData });
                        }

                        uploadFile = (await _unitofWork.UploadFileInfo.GetAllAsync(x => x.BankBranchCode == department.BankBranchCode && x.ResponseOrganizationCode == organizationClaim.OrganizationCode && x.Status == "Response" && x.ResponseUserCode == userInClaim.UserCode, includeProperties: "Organization,User")).ToList();


                    }
                }
                else
                {
                    uploadFile = (await _unitofWork.UploadFileInfo.GetAllAsync(includeProperties: "Organization,User")).ToList();

                }
                var response = uploadFile.Select(x => new BankFetchDataVM
                {

                    Id = x.Id,
                    FileInfoCode = x.FileInfoCode,
                    FileNumber = x.FileNumber,
                    FileName = x.FileName,
                    BankName = x.BankName,
                    BranchName = x.BranchName,
                    IFSC = x.IFSC,
                    OrganizationCode = x.OrganizationCode,
                    Organization = x.Organization,
                    FilePath = x.RequestFilePath,
                    AcknowledgementFileName = x.AcknowledgementFileName,
                    BankAcknowledgementFilePath = x.BankAcknowledgementFilePath,
                    NoAcknowledgementFileName = x.NoAcknowledgementFileName,
                    BankNoAcknowledgementFilePath = x.BankNoAcknowledgementFilePath,
                    BankResponsePath = x.BankResponsePath,
                    ResponseFileName = x.ResponseFileName,
                    ResponseDate = x.ResponseDate,
                    Status = x.Status,
                    DepartmentApprovalStatus = x.DepartmentApprovalStatus,
                    IsInternalOnly = x.IsInternalOnly,
                    InprocessDate = x.InprocessDate,
                    ResponseUserCode = x.ResponseUserCode,
                    ResponseOrganizationCode = x.ResponseOrganizationCode,
                    TotalCount = x.TotalCount,
                    FailedCount = x.FailedCount,
                    ProcessedCount = x.ProcessedCount,
                    OfficeName = x.OfficeName,
                    UserCode = x.UserCode,
                    User = x.User
                }).ToList();
                var encryptedResponse = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, response)));
                HttpContext.Response.Headers.Append("X-Data-Hash", encryptedResponse.Hash);
                return Ok(new { data = encryptedResponse.EncryptedData });
            }
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new
                {
                    data = data.EncryptedData
                });
            }
        }
        [HttpGet("GetPendingFileByBankUser")]
        public async Task<IActionResult> GetFileByBankUser()
        {
            try
            {
                var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                List<UploadFileInfo> uploadFile = new List<UploadFileInfo>();
                if (userInClaim.IsEntityUser)
                {
                    var organizationClaim = await _unitofWork.Organization.FirstOrDefaultAsync(x =>
                      x.OrganizationCode == User.FindFirst(SD.OrganizationCode).Value);

                    var adminuser = await _unitofWork.AdministorUser.FirstOrDefaultAsync(
                        x => x.UserCode == userInClaim.UserCode);
                    if (adminuser != null)
                    {
                        uploadFile = (await _unitofWork.UploadFileInfo.GetAllAsync(x => x.BankName == organizationClaim.OrganizationName && x.ResponseOrganizationCode == organizationClaim.OrganizationCode && x.Status == "InProcess", includeProperties: "Organization,User")).ToList();

                    }
                    else
                    {
                        var department = await _unitofWork.BankUser
                    .FirstOrDefaultAsync(x => x.BankUserCode == User.FindFirst(SD.UserTypeCode).Value, includeProperties: "BankBranch");

                        if (department == null)
                        {
                            var err = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));

                            HttpContext.Response.Headers.Append("X-Data-Hash", err.Hash);
                            return NotFound(new { data = err.EncryptedData });
                        }

                        uploadFile = (await _unitofWork.UploadFileInfo.GetAllAsync(x => x.BranchName == department.BankBranch.BranchName && x.IFSC == department.BankBranch.IFSC && x.BankName == department.BankBranch.BankName && x.Status == "InProcess", includeProperties: "Organization,User")).ToList();


                    }
                }
                else
                {
                    uploadFile = (await _unitofWork.UploadFileInfo.GetAllAsync(includeProperties: "Organization,User")).ToList();

                }
                var response = uploadFile.Select(x => new BankFetchDataVM
                {
                    Id = x.Id,
                    FileInfoCode = x.FileInfoCode,
                    FileNumber = x.FileNumber,
                    FileName = x.FileName,
                    BankName = x.BankName,
                    BranchName = x.BranchName,
                    IFSC = x.IFSC,
                    OrganizationCode = x.OrganizationCode,
                    Organization = x.Organization,
                    FilePath = x.BankProcessFilePath,
                    Status = x.Status,
                    DepartmentApprovalStatus = x.DepartmentApprovalStatus,
                    IsInternalOnly = x.IsInternalOnly,
                    InprocessDate = x.InprocessDate,
                    ResponseUserCode = x.ResponseUserCode,
                    ResponseOrganizationCode = x.ResponseOrganizationCode,
                    TotalCount = x.TotalCount,
                    FailedCount = x.FailedCount,
                    ProcessedCount = x.ProcessedCount,
                    FileSize = x.FileSize,
                    OfficeName = x.OfficeName,
                    UserCode = x.UserCode,
                    User = x.User
                }).ToList();

                var encryptedResponse = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, response)));
                HttpContext.Response.Headers.Append("X-Data-Hash", encryptedResponse.Hash);
                return Ok(new { data = encryptedResponse.EncryptedData });
            }
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new
                {
                    data = data.EncryptedData
                });
            }
        }
        [HttpGet("pendingFileByUser")]
        public async Task<IActionResult> PendingFile()
        {
            try
            {
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                var roleCode = User.FindFirstValue(ClaimTypes.Role);
                var orgCode = User.FindFirst(SD.OrganizationCode)?.Value;
                var routeVMList = new List<GetUploadFileInfoVM>();
                List<UploadFileInfo> uploadFile = new List<UploadFileInfo>();
                if (userInClaim.IsEntityUser)
                {
                    var organizationClaim = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == User.FindFirst(SD.OrganizationCode).Value);
                    var adminuser = await _unitofWork.AdministorUser.FirstOrDefaultAsync(x => x.UserCode == userInClaim.UserCode);
                    if (adminuser == null)
                    {
                        var latestHistories = (await _unitofWork.UploadFileInfoHistory.GetAllAsync()).GroupBy(x => x.FileInfoCode).Select(g => g.OrderByDescending(x => x.Id).First()).ToList();
                        var history = latestHistories.Where(h => h.AssignedToUserCode == userInClaim.UserCode && h.AssignedToRoleCode == roleCode).ToList();
                        var codes = history.Select(h => h.FileInfoCode).Distinct().ToList();
                        uploadFile = (await _unitofWork.UploadFileInfo.GetAllAsync(x => codes.Contains(x.FileInfoCode) && x.OrganizationCode == orgCode && x.IsInternalOnly == false && x.DepartmentApprovalStatus == "Pending" && x.Status == "Requested", includeProperties: "Organization,User")).ToList();
                    }
                    else
                    {
                        uploadFile = (await _unitofWork.UploadFileInfo.GetAllAsync(x => x.Status == "Requested" && x.OrganizationCode == organizationClaim.OrganizationCode && x.IsInternalOnly == false, includeProperties: "Organization,User")).ToList();
                    }
                }
                else
                {
                    uploadFile = (await _unitofWork.UploadFileInfo.GetAllAsync(x => x.Status == "InProcess" && x.IsInternalOnly == false, includeProperties: "Organization,User")).ToList();
                }
                foreach (var file in uploadFile)
                {
                    var historyList = await _unitofWork.UploadFileInfoHistory.GetAllAsync(h => h.FileInfoCode == file.FileInfoCode);
                    var getSourceOrganization = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == file.OrganizationCode);
                    var historyVM = new List<FileInfoHistoryVM>();
                    foreach (var h in historyList)
                    {
                        var actionByUser = await _unitofWork.User.FirstOrDefaultAsync(u => u.UserCode == h.ActionByUserCode);
                        var actionByRole = await _unitofWork.UserRole.FirstOrDefaultAsync(r => r.RoleCode == h.ActionByRoleCode);
                        var assignedUser = await _unitofWork.User.FirstOrDefaultAsync(u => u.UserCode == h.AssignedToUserCode);
                        var assignedRole = await _unitofWork.UserRole.FirstOrDefaultAsync(r => r.RoleCode == h.AssignedToRoleCode);
                        historyVM.Add(new FileInfoHistoryVM
                        {
                            UploadFileInfoHistoryCode = h.UploadFileInfoHistoryCode,
                            FileInfoCode = h.FileInfoCode,
                            UploadFileInfo = file,
                            ActionByUserCode = h.ActionByUserCode,
                            ActionByUser = actionByUser,
                            ActionByRoleCode = h.ActionByRoleCode,
                            ActionByRole = actionByRole,
                            AssignedToUserCode = h.AssignedToUserCode,
                            AssignedToUser = assignedUser,
                            AssignedToRoleCode = h.AssignedToRoleCode,
                            AssignedToRole = assignedRole,
                            Status = h.Status,
                            ActionDate = h.ActionDate
                        });
                    }
                    routeVMList.Add(new GetUploadFileInfoVM
                    {
                        FileInfoCode = file.FileInfoCode,
                        FileNumber = file.FileNumber,
                        FileName = file.FileName,
                        FilePath = file.RequestFilePath,
                        BankName = file.BankName,
                        BranchName = file.BranchName,
                        IFSC = file.IFSC,
                        OrganizationCode = file.OrganizationCode,
                        Organization = file.Organization,
                        Status = file.Status,
                        DepartmentApprovalStatus = file.DepartmentApprovalStatus,
                        IsInternalOnly = file.IsInternalOnly,
                        RequestedDate = file.RequestedDate,
                        InprocessDate = file.InprocessDate,
                        ResponseDate = file.ResponseDate,
                        ResponseUserCode = file.ResponseUserCode,
                        ResponseOrganizationCode = file.ResponseOrganizationCode,
                        TotalCount = file.TotalCount,
                        FailedCount = file.FailedCount,
                        ProcessedCount = file.ProcessedCount,
                        UserCode = file.UserCode,
                        User = file.User,

                        OfficeCode = file.OfficeCode,
                        FileSize = file.FileSize,
                        OfficeName = file.OfficeName,
                        FileInfoHistory = historyVM
                    });
                }

                var encryptedResponse = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, routeVMList)));
                HttpContext.Response.Headers.Append("X-Data-Hash", encryptedResponse.Hash);
                return Ok(new { data = encryptedResponse.EncryptedData });
            }
            catch (Exception ex)
            {
                var errorResponse = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", errorResponse.Hash);
                return StatusCode(500, new { data = errorResponse.EncryptedData });
            }
        }
        [HttpGet("GetInprocessFile")]
        public async Task<IActionResult> GetInprocessFile()
        {
            try
            {
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                var routeVMList = new List<GetUploadFileInfoVM>();
                IEnumerable<UploadFileInfo> uploadFile = new List<UploadFileInfo>();
                if (userInClaim.IsEntityUser)
                {
                    var organizationClaim = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == User.FindFirst(SD.OrganizationCode).Value);
                    var adminuser = await _unitofWork.AdministorUser.FirstOrDefaultAsync(x => x.UserCode == userInClaim.UserCode);
                    if (adminuser == null)
                    {
                        var department = await _unitofWork.Office.FirstOrDefaultAsync(x => x.OfficeCode == User.FindFirst(SD.EntityCode).Value, includeProperties: "Organization");

                        if (department == null)
                        {
                            var err = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", err.Hash);
                            return NotFound(new { data = err.EncryptedData });
                        }
                        uploadFile = await _unitofWork.UploadFileInfo.GetAllAsync(x => x.OrganizationCode == organizationClaim.OrganizationCode && x.OfficeName == department.OfficeName && x.Status == "InProcess", includeProperties: "Organization,User");
                    }
                    else
                    {
                        uploadFile = await _unitofWork.UploadFileInfo.GetAllAsync(x => x.OrganizationCode == organizationClaim.OrganizationCode && x.Status == "InProcess", includeProperties: "Organization,User");
                    }

                }
                else
                {
                    uploadFile = await _unitofWork.UploadFileInfo.GetAllAsync(x => x.Status == "InProcess", includeProperties: "Organization,User");
                }

                foreach (var file in uploadFile)
                {
                    var historyList = await _unitofWork.UploadFileInfoHistory.GetAllAsync(h => h.FileInfoCode == file.FileInfoCode);
                    var getSourceOrganization = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == file.OrganizationCode);
                    var historyVM = new List<FileInfoHistoryVM>();
                    foreach (var h in historyList)
                    {
                        var actionByUser = await _unitofWork.User.FirstOrDefaultAsync(u => u.UserCode == h.ActionByUserCode);
                        var actionByRole = await _unitofWork.UserRole.FirstOrDefaultAsync(r => r.RoleCode == h.ActionByRoleCode);
                        var assignedUser = await _unitofWork.User.FirstOrDefaultAsync(u => u.UserCode == h.AssignedToUserCode);
                        var assignedRole = await _unitofWork.UserRole.FirstOrDefaultAsync(r => r.RoleCode == h.AssignedToRoleCode);
                        historyVM.Add(new FileInfoHistoryVM
                        {
                            UploadFileInfoHistoryCode = h.UploadFileInfoHistoryCode,
                            FileInfoCode = h.FileInfoCode,
                            UploadFileInfo = file,
                            ActionByUserCode = h.ActionByUserCode,
                            ActionByUser = actionByUser,
                            ActionByRoleCode = h.ActionByRoleCode,
                            ActionByRole = actionByRole,
                            AssignedToUserCode = h.AssignedToUserCode,
                            AssignedToUser = assignedUser,
                            AssignedToRoleCode = h.AssignedToRoleCode,
                            AssignedToRole = assignedRole,
                            Status = h.Status,
                            ActionDate = h.ActionDate
                        });
                    }

                    routeVMList.Add(new GetUploadFileInfoVM
                    {
                        FileInfoCode = file.FileInfoCode,
                        FileNumber = file.FileNumber,
                        FileName = file.FileName,
                        BankName = file.BankName,
                        BranchName = file.BranchName,
                        IFSC = file.IFSC,
                        OrganizationCode = file.OrganizationCode,
                        Organization = file.Organization,
                        FilePath = file.RequestFilePath,
                        Status = file.Status,
                        DepartmentApprovalStatus = file.DepartmentApprovalStatus,
                        IsInternalOnly = file.IsInternalOnly,
                        RequestedDate = file.RequestedDate,
                        InprocessDate = file.InprocessDate,
                        ResponseDate = file.ResponseDate,
                        ResponseUserCode = file.ResponseUserCode,
                        ResponseOrganizationCode = file.ResponseOrganizationCode,
                        TotalCount = file.TotalCount,
                        FailedCount = file.FailedCount,
                        ProcessedCount = file.ProcessedCount,
                        UserCode = file.UserCode,
                        User = file.User,
                        FileInfoHistory = historyVM
                    });
                }

                var encryptedResponse = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, routeVMList)));
                HttpContext.Response.Headers.Append("X-Data-Hash", encryptedResponse.Hash);
                return Ok(new { data = encryptedResponse.EncryptedData });
            }
            catch (Exception ex)
            {
                var errorResponse = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", errorResponse.Hash);
                return StatusCode(500, new { data = errorResponse.EncryptedData });
            }
        }
        [HttpGet("GetResponsefileBydepartmentUser")]
        public async Task<IActionResult> GetResponfile()
        {
            try
            {
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                var routeVMList = new List<GetUploadFileInfoVM>();
                IEnumerable<UploadFileInfo> uploadFile = new List<UploadFileInfo>();
                if (userInClaim.IsEntityUser)
                {
                    var organizationClaim = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == User.FindFirst(SD.OrganizationCode).Value);
                    var adminuser = await _unitofWork.AdministorUser.FirstOrDefaultAsync(x => x.UserCode == userInClaim.UserCode);
                    if (adminuser == null)
                    {
                        var department = await _unitofWork.Office.FirstOrDefaultAsync(x => x.OfficeCode == User.FindFirst(SD.EntityCode).Value, includeProperties: "Department");
                        if (department == null)
                        {
                            var err = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", err.Hash);
                            return NotFound(new { data = err.EncryptedData });
                        }
                        uploadFile = await _unitofWork.UploadFileInfo.GetAllAsync(x => x.OrganizationCode == organizationClaim.OrganizationCode && x.OfficeName == department.OfficeName && x.Status == "Response", includeProperties: "Organization,User");
                    }
                    else
                    {
                        uploadFile = await _unitofWork.UploadFileInfo.GetAllAsync(x => x.OrganizationCode == organizationClaim.OrganizationCode && x.Status == "Response", includeProperties: "Organization,User");
                    }
                }
                else
                {
                    uploadFile = await _unitofWork.UploadFileInfo.GetAllAsync(x => x.Status == "Response", includeProperties: "Organization,User");
                }
                foreach (var file in uploadFile)
                {
                    var historyList = await _unitofWork.UploadFileInfoHistory.GetAllAsync(h => h.FileInfoCode == file.FileInfoCode);
                    var getSourceOrganization = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == file.OrganizationCode);
                    var historyVM = new List<FileInfoHistoryVM>();
                    foreach (var h in historyList)
                    {
                        var actionByUser = await _unitofWork.User.FirstOrDefaultAsync(u => u.UserCode == h.ActionByUserCode);
                        var actionByRole = await _unitofWork.UserRole.FirstOrDefaultAsync(r => r.RoleCode == h.ActionByRoleCode);
                        var assignedUser = await _unitofWork.User.FirstOrDefaultAsync(u => u.UserCode == h.AssignedToUserCode);
                        var assignedRole = await _unitofWork.UserRole.FirstOrDefaultAsync(r => r.RoleCode == h.AssignedToRoleCode);
                        historyVM.Add(new FileInfoHistoryVM
                        {
                            UploadFileInfoHistoryCode = h.UploadFileInfoHistoryCode,
                            FileInfoCode = h.FileInfoCode,
                            UploadFileInfo = file,
                            ActionByUserCode = h.ActionByUserCode,
                            ActionByUser = actionByUser,
                            ActionByRoleCode = h.ActionByRoleCode,
                            ActionByRole = actionByRole,
                            AssignedToUserCode = h.AssignedToUserCode,
                            AssignedToUser = assignedUser,
                            AssignedToRoleCode = h.AssignedToRoleCode,
                            AssignedToRole = assignedRole,
                            Status = h.Status,
                            ActionDate = h.ActionDate
                        });
                    }

                    routeVMList.Add(new GetUploadFileInfoVM
                    {
                        FileInfoCode = file.FileInfoCode,
                        FileNumber = file.FileNumber,
                        FileName = file.FileName,
                        BankName = file.BankName,
                        BranchName = file.BranchName,
                        IFSC = file.IFSC,
                        OrganizationCode = file.OrganizationCode,
                        Organization = file.Organization,
                        FilePath = file.ResponseFilePath,
                        Status = file.Status,
                        DepartmentApprovalStatus = file.DepartmentApprovalStatus,
                        IsInternalOnly = file.IsInternalOnly,
                        RequestedDate = file.RequestedDate,
                        InprocessDate = file.InprocessDate,
                        ResponseDate = file.ResponseDate,
                        ResponseUserCode = file.ResponseUserCode,
                        ResponseOrganizationCode = file.ResponseOrganizationCode,
                        TotalCount = file.TotalCount,
                        FailedCount = file.FailedCount,
                        ProcessedCount = file.ProcessedCount,
                        UserCode = file.UserCode,
                        User = file.User,
                        FileInfoHistory = historyVM
                    });
                }

                var encryptedResponse = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, routeVMList)));
                HttpContext.Response.Headers.Append("X-Data-Hash", encryptedResponse.Hash);
                return Ok(new { data = encryptedResponse.EncryptedData });
            }
            catch (Exception ex)
            {
                var errorResponse = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", errorResponse.Hash);
                return StatusCode(500, new { data = errorResponse.EncryptedData });
            }
        }
        [HttpGet("getRequestFile")]
        public async Task<IActionResult> GetRequestFile()
        {
            try
            {
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                var routeVMList = new List<GetUploadFileInfoVM>();
                List<UploadFileInfo> uploadFile = new List<UploadFileInfo>();
                if (userInClaim.IsEntityUser)
                {
                    var organizationClaim = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == User.FindFirst(SD.OrganizationCode).Value);
                    var adminuser = await _unitofWork.AdministorUser.FirstOrDefaultAsync(x => x.UserCode == userInClaim.UserCode);
                    if (adminuser == null)
                    {
                        var department = await _unitofWork.Office.FirstOrDefaultAsync(x => x.OfficeCode == User.FindFirst(SD.EntityCode).Value, includeProperties: "Organization");
                        if (department == null)
                        {
                            var err = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", err.Hash);
                            return NotFound(new { data = err.EncryptedData });
                        }
                        uploadFile = (await _unitofWork.UploadFileInfo.GetAllAsync(x => x.OrganizationCode == organizationClaim.OrganizationCode && x.OfficeName == department.OfficeName && x.Status == "Request", includeProperties: "Organization,User")).ToList();
                    }
                    else
                    {
                        uploadFile = (await _unitofWork.UploadFileInfo.GetAllAsync(x => x.OrganizationCode == organizationClaim.OrganizationCode && x.Status == "Request", includeProperties: "Organization,User")).ToList();
                    }
                }
                else
                {
                    uploadFile = (await _unitofWork.UploadFileInfo.GetAllAsync(x => x.Status == "Request", includeProperties: "Organization,User")).ToList();
                }

                foreach (var file in uploadFile)
                {
                    var historyList = await _unitofWork.UploadFileInfoHistory.GetAllAsync(h => h.FileInfoCode == file.FileInfoCode);
                    var getSourceOrganization = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == file.OrganizationCode);
                    var historyVM = new List<FileInfoHistoryVM>();
                    foreach (var h in historyList)
                    {
                        var actionByUser = await _unitofWork.User.FirstOrDefaultAsync(u => u.UserCode == h.ActionByUserCode);
                        var actionByRole = await _unitofWork.UserRole.FirstOrDefaultAsync(r => r.RoleCode == h.ActionByRoleCode);
                        var assignedUser = await _unitofWork.User.FirstOrDefaultAsync(u => u.UserCode == h.AssignedToUserCode);
                        var assignedRole = await _unitofWork.UserRole.FirstOrDefaultAsync(r => r.RoleCode == h.AssignedToRoleCode);
                        historyVM.Add(new FileInfoHistoryVM
                        {
                            UploadFileInfoHistoryCode = h.UploadFileInfoHistoryCode,
                            FileInfoCode = h.FileInfoCode,
                            UploadFileInfo = file,
                            ActionByUserCode = h.ActionByUserCode,
                            ActionByUser = actionByUser,
                            ActionByRoleCode = h.ActionByRoleCode,
                            ActionByRole = actionByRole,
                            AssignedToUserCode = h.AssignedToUserCode,
                            AssignedToUser = assignedUser,
                            AssignedToRoleCode = h.AssignedToRoleCode,
                            AssignedToRole = assignedRole,
                            Status = h.Status,
                            ActionDate = h.ActionDate
                        });
                    }

                    routeVMList.Add(new GetUploadFileInfoVM
                    {
                        FileInfoCode = file.FileInfoCode,
                        FileNumber = file.FileNumber,
                        FileName = file.FileName,
                        BankName = file.BankName,
                        BranchName = file.BranchName,
                        IFSC = file.IFSC,
                        OrganizationCode = file.OrganizationCode,
                        Organization = file.Organization,
                        FilePath = file.RequestFilePath,
                        Status = file.Status,
                        DepartmentApprovalStatus = file.DepartmentApprovalStatus,
                        IsInternalOnly = file.IsInternalOnly,
                        RequestedDate = file.RequestedDate,
                        InprocessDate = file.InprocessDate,
                        ResponseDate = file.ResponseDate,
                        ResponseUserCode = file.ResponseUserCode,
                        ResponseOrganizationCode = file.ResponseOrganizationCode,
                        TotalCount = file.TotalCount,
                        FailedCount = file.FailedCount,
                        FileSize = file.FileSize,
                        ProcessedCount = file.ProcessedCount,
                        UserCode = file.UserCode,
                        User = file.User,
                        FileInfoHistory = historyVM
                    });
                }

                var encryptedResponse = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, routeVMList)));
                HttpContext.Response.Headers.Append("X-Data-Hash", encryptedResponse.Hash);
                return Ok(new { data = encryptedResponse.EncryptedData });
            }
            catch (Exception ex)
            {
                var errorResponse = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", errorResponse.Hash);
                return StatusCode(500, new { data = errorResponse.EncryptedData });
            }
        }
        [HttpGet("getAllInternalFile")]
        public async Task<IActionResult> GetAllInternalFile()
        {
            try
            {
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                var routeVMList = new List<GetUploadFileInfoVM>();
                IEnumerable<UploadFileInfo> uploadFile = new List<UploadFileInfo>();
                if (userInClaim.IsEntityUser)
                {
                    var organizationClaim = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == User.FindFirst(SD.OrganizationCode).Value);

                    uploadFile = await _unitofWork.UploadFileInfo.GetAllAsync(x => x.OrganizationCode == organizationClaim.OrganizationCode && x.IsInternalOnly == true, includeProperties: "Organization,User");
                }
                else
                {
                    uploadFile = await _unitofWork.UploadFileInfo.GetAllAsync(x => x.IsInternalOnly == true, includeProperties: "Organization,User");
                }
                var filteredFiles = uploadFile.Where(file =>
                {
                    if (file.UserCode == userInClaim.UserCode)
                        return true;

                    if (!string.IsNullOrWhiteSpace(file.SharedUsers))
                    {
                        try
                        {
                            var sharedUsers = string.IsNullOrWhiteSpace(file.SharedUsers)
                          ? new List<SharedUser>()
                          : new List<SharedUser>
                          {
                                JsonSerializer.Deserialize<SharedUser>(file.SharedUsers)
                          }; return sharedUsers.Any(su => su.UserCode != null && su.UserCode.Contains(userInClaim.UserCode));
                        }
                        catch
                        {
                            return false;
                        }
                    }

                    return false;
                }).ToList();

                foreach (var file in filteredFiles)
                {
                    var historyList = await _unitofWork.UploadFileInfoHistory.GetAllAsync(h => h.FileInfoCode == file.FileInfoCode);
                    var getSourceOrganization = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == file.OrganizationCode);
                    var historyVM = new List<FileInfoHistoryVM>();
                    foreach (var h in historyList)
                    {
                        var actionByUser = await _unitofWork.User.FirstOrDefaultAsync(u => u.UserCode == h.ActionByUserCode);
                        var actionByRole = await _unitofWork.UserRole.FirstOrDefaultAsync(r => r.RoleCode == h.ActionByRoleCode);
                        var assignedUser = await _unitofWork.User.FirstOrDefaultAsync(u => u.UserCode == h.AssignedToUserCode);
                        var assignedRole = await _unitofWork.UserRole.FirstOrDefaultAsync(r => r.RoleCode == h.AssignedToRoleCode);
                        historyVM.Add(new FileInfoHistoryVM
                        {
                            UploadFileInfoHistoryCode = h.UploadFileInfoHistoryCode,
                            FileInfoCode = h.FileInfoCode,
                            UploadFileInfo = file,
                            ActionByUserCode = h.ActionByUserCode,
                            ActionByUser = actionByUser,
                            ActionByRoleCode = h.ActionByRoleCode,
                            ActionByRole = actionByRole,
                            AssignedToUserCode = h.AssignedToUserCode,
                            AssignedToUser = assignedUser,
                            AssignedToRoleCode = h.AssignedToRoleCode,
                            AssignedToRole = assignedRole,
                            Status = h.Status,
                            ActionDate = h.ActionDate
                        });
                    }
                    routeVMList.Add(new GetUploadFileInfoVM
                    {
                        FileInfoCode = file.FileInfoCode,
                        FileNumber = file.FileNumber,
                        FileName = file.FileName,
                        BankName = file.BankName,
                        BranchName = file.BranchName,
                        SharedUsers = string.IsNullOrWhiteSpace(file.SharedUsers) ? new List<SharedUser>() : new List<SharedUser>
                            {
                                JsonSerializer.Deserialize<SharedUser>(file.SharedUsers)
                            },
                        IFSC = file.IFSC,
                        OrganizationCode = file.OrganizationCode,
                        Organization = file.Organization,
                        FilePath = file.InternalFilePath,
                        Status = file.Status,
                        DepartmentApprovalStatus = file.DepartmentApprovalStatus,
                        IsInternalOnly = file.IsInternalOnly,
                        RequestedDate = file.RequestedDate,
                        InprocessDate = file.InprocessDate,
                        ResponseDate = file.ResponseDate,
                        ResponseUserCode = file.ResponseUserCode,
                        ResponseOrganizationCode = file.ResponseOrganizationCode,
                        TotalCount = file.TotalCount,
                        FailedCount = file.FailedCount,
                        FileSize = file.FileSize,
                        ProcessedCount = file.ProcessedCount,
                        UserCode = file.UserCode,
                        User = file.User,
                        FileInfoHistory = historyVM
                    });
                }

                var encryptedResponse = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, routeVMList)));
                HttpContext.Response.Headers.Append("X-Data-Hash", encryptedResponse.Hash);
                return Ok(new { data = encryptedResponse.EncryptedData });
            }
            catch (Exception ex)
            {
                var errorResponse = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", errorResponse.Hash);
                return StatusCode(500, new { data = errorResponse.EncryptedData });
            }
        }
        [HttpPost("getinternalfilewithexplorer")]
        public async Task<IActionResult> OpenExplorerSystem([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);
                if (decryptedData != null)
                {
                    StringValueVM requestData = JsonSerializer.Deserialize<StringValueVM>(decryptedData);
                    var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                    if (string.IsNullOrWhiteSpace(requestData.Value))
                    {

                        var rootFolders = await _unitofWork.UserFileDirectory.GetAllAsync(x => x.ParentDirectoryCode == null && x.CreatedByUserCode == userInClaim.UserCode);
                        var rootFiles = await _unitofWork.UploadFileInfo.GetAllAsync(x => x.UserFileDirectoryCode == null && x.IsInternalOnly);
                        var myFiles = new List<object>();
                        var sharedFiles = new List<object>();
                        foreach (var file in rootFiles)
                        {
                            bool isOwner = file.UserCode == userInClaim.UserCode;
                            bool isShared = false;

                            if (!string.IsNullOrWhiteSpace(file.SharedUsers))
                            {
                                var sharedUsers = string.IsNullOrWhiteSpace(file.SharedUsers)
                                    ? new List<SharedUser>()
                                    : new List<SharedUser>
                                    {
      JsonSerializer.Deserialize<SharedUser>(file.SharedUsers)
                                    };

                                isShared = sharedUsers.Any(su =>
                                    su.UserCode != null &&
                                    su.UserCode.Any(u => string.Equals(u?.Trim(),
                                                                      userInClaim.UserCode?.Trim(),
                                                                      StringComparison.OrdinalIgnoreCase))
                                );
                            }

                            if (!isOwner && !isShared) continue;


                            if (!isOwner && !isShared) continue;

                            var fileObj = new
                            {
                                fileCode = file.FileInfoCode,
                                fileName = file.FileName,
                                size = file.FileSize,
                                date = file.RequestedDate,
                                filePath = file.InternalFilePath,
                                SharedUsers = string.IsNullOrWhiteSpace(file.SharedUsers) ? new List<SharedUser>() : new List<SharedUser>
                      {
                          JsonSerializer.Deserialize<SharedUser>(file.SharedUsers)
                      },
                            };

                            if (isOwner) myFiles.Add(fileObj);
                            else sharedFiles.Add(fileObj);
                        }
                        var datagroupt = new
                        {
                            currentDirectory = (string)null,
                            breadcrumb = new[] { new { code = (string)null, name = "Home" } },

                            folders = rootFolders.Select(f => new
                            {
                                code = f.UserFileDirectoryCode,
                                name = f.DirectoryName,
                                createdOn = f.CreatedDate,
                            }),

                            myFiles,
                            sharedFiles
                        };

                        var datas = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                      (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, datagroupt)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", datas.Hash);
                        return Ok(new { data = datas.EncryptedData });
                    }
                    var currentDir = await _unitofWork.UserFileDirectory.FirstOrDefaultAsync(x => x.UserFileDirectoryCode == requestData.Value);

                    var folders = await _unitofWork.UserFileDirectory.GetAllAsync(
                        x => x.ParentDirectoryCode == requestData.Value && !x.IsDeleted);

                    var files = await _unitofWork.UploadFileInfo.GetAllAsync(
                        x => x.UserFileDirectoryCode == requestData.Value && x.IsInternalOnly);

                    var myFilesList = new List<object>();
                    var sharedFilesList = new List<object>();

                    foreach (var file in files)
                    {
                        bool isOwner = file.UserCode == userInClaim.UserCode;
                        bool isShared = false;

                        if (!string.IsNullOrWhiteSpace(file.SharedUsers))
                        {
                            try
                            {
                                var sharedUsers = JsonSerializer.Deserialize<List<string>>(file.SharedUsers);
                                isShared = sharedUsers != null && sharedUsers.Contains(userInClaim.UserCode);
                            }
                            catch { }
                        }

                        if (!isOwner && !isShared) continue;

                        var fileObj = new
                        {
                            fileCode = file.FileInfoCode,
                            fileName = file.FileName,
                            size = file.FileSize,
                            filePath = file.InternalFilePath,
                            date = file.RequestedDate,
                            SharedUsers = string.IsNullOrWhiteSpace(file.SharedUsers) ? new List<SharedUser>() : new List<SharedUser>
                      {
                          JsonSerializer.Deserialize<SharedUser>(file.SharedUsers)
                      },
                        };

                        if (isOwner) myFilesList.Add(fileObj);
                        else sharedFilesList.Add(fileObj);
                    }
                    var breadcrumb = new List<object>();
                    var temp = currentDir;

                    while (temp != null)
                    {
                        breadcrumb.Insert(0, new
                        {
                            code = temp.UserFileDirectoryCode,
                            name = temp.DirectoryName
                        });

                        if (temp.ParentDirectoryCode == null)
                            break;

                        temp = await _unitofWork.UserFileDirectory
                            .FirstOrDefaultAsync(x => x.UserFileDirectoryCode == temp.ParentDirectoryCode);
                    }

                    breadcrumb.Insert(0, new { code = (string)null, name = "Home" });
                    var datagroup = (new
                    {
                        currentDirectory = requestData.Value,
                        breadcrumb,

                        folders = folders.Select(f => new
                        {
                            code = f.UserFileDirectoryCode,
                            name = f.DirectoryName,
                            createdOn = f.CreatedDate,
                        }),

                        myFiles = myFilesList,
                        sharedFiles = sharedFilesList
                    });
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                          (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, datagroup)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return Ok(new { data = data.EncryptedData });
                }
                else
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                           (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });


                }
            }
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                               (_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new
                {
                    data = data.EncryptedData
                });
            }
        }
        [HttpGet("getAllInternalFileByBankUser")]
        public async Task<IActionResult> GetAllInternalFileByBankUser()
        {
            try
            {
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                var routeVMList = new List<GetUploadFileInfoVM>();
                IEnumerable<UploadFileInfo> uploadFile = new List<UploadFileInfo>();
                if (userInClaim.IsEntityUser)
                {
                    var organizationClaim = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == User.FindFirst(SD.OrganizationCode).Value);
                    var adminuser = await _unitofWork.AdministorUser.FirstOrDefaultAsync(x => x.UserCode == userInClaim.UserCode);
                    if (adminuser == null)
                    {
                        var department = await _unitofWork.BankBranch.FirstOrDefaultAsync(x => x.BankBranchCode == User.FindFirst(SD.EntityCode).Value, includeProperties: "Organization");
                        if (department == null)
                        {
                            var err = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", err.Hash);
                            return NotFound(new { data = err.EncryptedData });
                        }
                        uploadFile = await _unitofWork.UploadFileInfo.GetAllAsync(x => x.OrganizationCode == organizationClaim.OrganizationCode && x.BankBranchCode == department.BankBranchCode && x.IsInternalOnly == true, includeProperties: "Organization,User");
                    }
                    else
                    {
                        uploadFile = await _unitofWork.UploadFileInfo.GetAllAsync(x => x.OrganizationCode == organizationClaim.OrganizationCode && x.IsInternalOnly == true, includeProperties: "Organization,User");
                    }
                }
                else
                {
                    uploadFile = await _unitofWork.UploadFileInfo.GetAllAsync(x => x.IsInternalOnly == true, includeProperties: "Organization,User");
                }
                var filteredFiles = uploadFile.Where(file =>
                {
                    if (file.UserCode == userInClaim.UserCode)
                        return true;

                    if (!string.IsNullOrWhiteSpace(file.SharedUsers))
                    {
                        try
                        {
                            var sharedUsers = string.IsNullOrWhiteSpace(file.SharedUsers)
                          ? new List<SharedUser>()
                          : new List<SharedUser>
                          {
                                JsonSerializer.Deserialize<SharedUser>(file.SharedUsers)
                          };
                            return sharedUsers.Any(su => su.UserCode != null && su.UserCode.Contains(userInClaim.UserCode));
                        }
                        catch
                        {
                            return false;
                        }
                    }
                    return false;
                }).ToList();


                foreach (var file in filteredFiles)
                {
                    var historyList = await _unitofWork.UploadFileInfoHistory.GetAllAsync(h => h.FileInfoCode == file.FileInfoCode);
                    var getSourceOrganization = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == file.OrganizationCode);
                    routeVMList.Add(new GetUploadFileInfoVM
                    {
                        FileInfoCode = file.FileInfoCode,
                        FileNumber = file.FileNumber,
                        FileName = file.FileName,
                        BankName = file.BankName,
                        BranchName = file.BranchName,
                        IFSC = file.IFSC,
                        OrganizationCode = file.OrganizationCode,
                        Organization = file.Organization,
                        FilePath = file.InternalFilePath,
                        SharedUsers = string.IsNullOrWhiteSpace(file.SharedUsers)
                            ? new List<SharedUser>()
                            : new List<SharedUser>
                            {
                                JsonSerializer.Deserialize<SharedUser>(file.SharedUsers)
                            },
                        Status = file.Status,
                        DepartmentApprovalStatus = file.DepartmentApprovalStatus,
                        IsInternalOnly = file.IsInternalOnly,

                        RequestedDate = file.RequestedDate,
                        InprocessDate = file.InprocessDate,
                        ResponseDate = file.ResponseDate,
                        ResponseUserCode = file.ResponseUserCode,
                        ResponseOrganizationCode = file.ResponseOrganizationCode,
                        TotalCount = file.TotalCount,
                        FileSize = file.FileSize,
                        FailedCount = file.FailedCount,
                        ProcessedCount = file.ProcessedCount,
                        UserCode = file.UserCode,
                        User = file.User,
                    });
                }

                var encryptedResponse = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, routeVMList)));
                HttpContext.Response.Headers.Append("X-Data-Hash", encryptedResponse.Hash);
                return Ok(new { data = encryptedResponse.EncryptedData });
            }
            catch (Exception ex)
            {
                var errorResponse = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", errorResponse.Hash);
                return StatusCode(500, new { data = errorResponse.EncryptedData });
            }
        }
        [HttpPost("uploadInternalFileByBankUser")]
        public async Task<IActionResult> UploadInternalFileByBankUser([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);
                if (decryptedData != null)
                {
                    var user = JsonSerializer.Deserialize<UploadFileInfoVM>(decryptedData);
                    if (TryValidateModel(user))
                    {
                        var OfficeInClaim = await _unitofWork.BankBranch.FirstOrDefaultAsync(x => x.BankBranchCode == User.FindFirst(SD.EntityCode).Value);
                        var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                        var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                        var org = await _unitofWork.Organization.FirstOrDefaultAsync(d => d.OrganizationCode == User.FindFirstValue(SD.OrganizationCode));
                        var file = _unitofWork.Organization.DecrypteIFromFileWithFileNameBase64(user.UploadFile, user.FileName);
                        var (fileHeader, rows, filetype) = _unitofWork.UploadFileInfo.ReadFile(file);

                        if (file == null)
                        {
                            var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.failure, ResponseHandler.NotFound, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                            return NotFound(new { data = okdata.EncryptedData });
                        }

                        byte[] fileBytes;
                        using (var ms = new MemoryStream())
                        {
                            await file.CopyToAsync(ms);
                            fileBytes = ms.ToArray();
                        }
                        double fileSize = _unitofWork.UploadFileInfo.GetFileStorage(fileBytes);
                        object Getuser = null;

                        var departmentUser = await _unitofWork.BankUser.FirstOrDefaultAsync(x => x.UserCode == userInClaim.UserCode);
                        if (departmentUser != null)
                        {
                            Getuser = departmentUser;

                            double storageleft = departmentUser.UsedStorageSize + fileSize;
                            if (departmentUser.TotalStorageSize < storageleft)
                            {
                                var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.failure, ResponseHandler.NoStorage, null)));
                                HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                                return BadRequest(new { data = okdata.EncryptedData });
                            }

                        }
                        else
                        {
                            var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.failure, ResponseHandler.NoStorage, "Office User Not Found")));
                            HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                            return NotFound(new { data = okdata.EncryptedData });
                        }
                        string orgFolder = $"org-{OfficeInClaim.OrganizationCode}";
                        string officeFolder = $"office-{OfficeInClaim.BankBranchCode}";
                        string userFolder = $"user-{userInClaim.UserCode}";
                        string privacyFolder = "files";
                        string folderPath = $"{orgFolder}/{officeFolder}/{userFolder}/{privacyFolder}";
                        var s3Key = await _s3Helper.UploadtoS3(file, folderPath);
                        var singleRecord = new UploadFileInfo
                        {
                            FileInfoCode = _unitofWork.UploadFileInfo.GenrateUniqueCode(),
                            FileNumber = await _unitofWork.UploadFileInfo.GenerateFileNumberAsync(),
                            FileName = user.FileName,
                            OrganizationCode = user.OrganizationCode,
                            FileType = filetype,
                            Status = "InternalOnly",
                            BankBranchCode = OfficeInClaim?.BankBranchCode,
                            RequestedDate = DateTime.Now.ToLocalTime(),
                            FileSize = fileSize,
                            UserCode = userInClaim.UserCode,
                            IsInternalOnly = true,
                            UserFileDirectoryCode = user.UserFileDirectoryCode,
                            InternalFilePath = s3Key,
                        };
                        await _unitofWork.UploadFileInfo.AddAsync(singleRecord);
                        await AddStorageAsync(fileSize, Getuser);
                        var ok = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Created, singleRecord)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", ok.Hash);
                        return Ok(new { data = ok.EncryptedData });
                    }
                    else
                    {
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList())));
                        HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                        return BadRequest(new { data = data.EncryptedData });
                    }
                }
                else
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });
                }
            }
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new { data = data.EncryptedData });
            }
        }
        [HttpGet("getAllFileInfo")]
        public async Task<IActionResult> GetAllFileInfo()
        {
            try
            {
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                var routeVMList = new List<GetUploadFileInfoVM>();
                List<UploadFileInfo> uploadFile = new List<UploadFileInfo>();
                if (userInClaim.IsEntityUser)
                {
                    var organizationClaim = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == User.FindFirst(SD.OrganizationCode).Value);
                    var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d =>
                    d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                    var adminuser = await _unitofWork.AdministorUser.FirstOrDefaultAsync(x => x.UserCode == userInClaim.UserCode);
                    if (adminuser == null)
                    {
                        var department = await _unitofWork.Office.FirstOrDefaultAsync(x => x.OfficeCode == User.FindFirst(SD.EntityCode).Value, includeProperties: "Organization");
                        if (department == null)
                        {
                            var err = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", err.Hash);
                            return NotFound(new { data = err.EncryptedData });
                        }
                        var latestHistories = (await _unitofWork.UploadFileInfoHistory.GetAllAsync(h => h.ActionByUserCode == userInClaim.UserCode && h.ActionByRoleCode == userRoleInClaim.RoleCode)).GroupBy(x => x.FileInfoCode).ToList();
                        var codes = latestHistories.Select(h => h.Key).Distinct().ToList();
                        uploadFile = (await _unitofWork.UploadFileInfo.GetAllAsync(x => codes.Contains(x.FileInfoCode) && x.OrganizationCode == organizationClaim.OrganizationCode, includeProperties: "Organization,User")).ToList();
                    }
                    else
                    {
                        uploadFile = (await _unitofWork.UploadFileInfo.GetAllAsync(x => x.OrganizationCode == organizationClaim.OrganizationCode && x.IsInternalOnly == false, includeProperties: "Organization,User")).ToList();
                    }
                }
                else
                {
                    uploadFile = (await _unitofWork.UploadFileInfo.GetAllAsync(c => c.IsInternalOnly == false, includeProperties: "Organization,User")).ToList();
                }

                foreach (var file in uploadFile)
                {
                    var historyList = await _unitofWork.UploadFileInfoHistory.GetAllAsync(h => h.FileInfoCode == file.FileInfoCode);
                    var getSourceOrganization = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == file.OrganizationCode);
                    List<FileInfoHistoryVM> historyVM = new List<FileInfoHistoryVM>();
                    foreach (var h in historyList)
                    {
                        var actionByUser = await _unitofWork.User.FirstOrDefaultAsync(u => u.UserCode == h.ActionByUserCode);
                        var actionByRole = await _unitofWork.UserRole.FirstOrDefaultAsync(r => r.RoleCode == h.ActionByRoleCode);
                        var assignedUser = await _unitofWork.User.FirstOrDefaultAsync(u => u.UserCode == h.AssignedToUserCode);
                        var assignedRole = await _unitofWork.UserRole.FirstOrDefaultAsync(r => r.RoleCode == h.AssignedToRoleCode);
                        historyVM.Add(new FileInfoHistoryVM
                        {
                            UploadFileInfoHistoryCode = h.UploadFileInfoHistoryCode,
                            FileInfoCode = h.FileInfoCode,
                            UploadFileInfo = file,
                            ActionByUserCode = h.ActionByUserCode,
                            ActionByUser = actionByUser,
                            ActionByRoleCode = h.ActionByRoleCode,
                            ActionByRole = actionByRole,
                            AssignedToUserCode = h.AssignedToUserCode,
                            AssignedToUser = assignedUser,
                            AssignedToRoleCode = h.AssignedToRoleCode,
                            AssignedToRole = assignedRole,
                            Status = h.Status,
                            ActionDate = h.ActionDate
                        });
                    }
                    routeVMList.Add(new GetUploadFileInfoVM
                    {
                        FileInfoCode = file.FileInfoCode,
                        FileNumber = file.FileNumber,
                        FileName = file.FileName,
                        BankName = file.BankName,
                        BranchName = file.BranchName,
                        IFSC = file.IFSC,
                        OrganizationCode = file.OrganizationCode,
                        Organization = file.Organization,
                        Status = file.Status,
                        FilePath = file.Status == "Requested" ? file.RequestFilePath : file.Status == "InProcess" ? file.InProcessFilePath : file.Status == "Draft" ? file.RequestFilePath : file.Status == "Rejected" ? file.RejectFilePath : null,
                        DepartmentApprovalStatus = file.DepartmentApprovalStatus,
                        IsInternalOnly = file.IsInternalOnly,
                        RequestedDate = file.RequestedDate,
                        InprocessDate = file.InprocessDate,
                        ResponseDate = file.ResponseDate,
                        ResponseUserCode = file.ResponseUserCode,
                        ResponseOrganizationCode = file.ResponseOrganizationCode,
                        ResponseFileName = file.ResponseFileName,
                        ResponseFilePath = file.ResponseFilePath,
                        AcknowledgementFileName = file.AcknowledgementFileName,
                        AcknowledgementFileNamePath = file.AcknowledgementFileNamePath,
                        NoAcknowledgementFileName = file.NoAcknowledgementFileName,
                        NoAcknowledgementFileNamePath = file.NoAcknowledgementFileNamePath,
                        TotalCount = file.TotalCount,
                        FailedCount = file.FailedCount,
                        ProcessedCount = file.ProcessedCount,
                        UserCode = file.UserCode,
                        User = file.User,
                        OfficeCode = file.OfficeCode,
                        FileSize = file.FileSize,
                        OfficeName = file.OfficeName,
                        FileInfoHistory = historyVM
                    });
                }
                var encryptedResponse = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, routeVMList)));
                HttpContext.Response.Headers.Append("X-Data-Hash", encryptedResponse.Hash);
                return Ok(new { data = encryptedResponse.EncryptedData });
            }
            catch (Exception ex)
            {
                var errorResponse = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", errorResponse.Hash);
                return StatusCode(500, new { data = errorResponse.EncryptedData });
            }
        }
        [HttpGet("getAllFileByDepartmentApprovalStatus")]
        public async Task<IActionResult> GetAllFileByDepartmentApprovalStatus([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);
                if (decryptedData != null)
                {
                    StringValueVM requestData = JsonSerializer.Deserialize<StringValueVM>(decryptedData);
                    if (TryValidateModel(requestData))
                    {
                        var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                        var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                        List<UploadFileInfo> uploadFileInfos = new List<UploadFileInfo>();

                        if (userInClaim.IsEntityUser == true)
                        {
                            var organizationclaimcode = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == User.FindFirst(SD.OrganizationCode).Value);

                            var adminuser = await _unitofWork.AdministorUser.FirstOrDefaultAsync(x => x.UserCode == userInClaim.UserCode);
                            if (adminuser == null)
                            {
                                var bankBranchMap = await _unitofWork.Office.FirstOrDefaultAsync(x => x.OfficeCode == User.FindFirst(SD.EntityCode).Value);

                                if (bankBranchMap == null)
                                {
                                    var err = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                                                _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));

                                    HttpContext.Response.Headers.Append("X-Data-Hash", err.Hash);
                                    return NotFound(new { data = err.EncryptedData });
                                }

                                uploadFileInfos = (await _unitofWork.UploadFileInfo.GetAllAsync(x => x.OrganizationCode == organizationclaimcode.OrganizationCode && x.Status == requestData.Value, includeProperties: "Organization,User")).ToList();


                            }
                            else
                            {
                                uploadFileInfos = (await _unitofWork.UploadFileInfo.GetAllAsync(x => x.ResponseOrganizationCode == organizationclaimcode.OrganizationCode && x.Status == requestData.Value, includeProperties: "Organization,User")).ToList();

                            }
                        }
                        else
                        {
                            uploadFileInfos = (await _unitofWork.UploadFileInfo.GetAllAsync(x => x.Status == requestData.Value, includeProperties: "Organization,User")).ToList();

                        }
                        var response = uploadFileInfos.Select(x => new BankFetchDataVM
                        {
                            Id = x.Id,
                            FileInfoCode = x.FileInfoCode,
                            FileNumber = x.FileNumber,
                            FileName = x.FileName,
                            BankName = x.BankName,
                            BranchName = x.BranchName,
                            IFSC = x.IFSC,
                            OrganizationCode = x.OrganizationCode,
                            Organization = x.Organization,
                            FilePath = x.RequestFilePath,
                            Status = x.Status,
                            DepartmentApprovalStatus = x.DepartmentApprovalStatus,
                            IsInternalOnly = x.IsInternalOnly,
                            InprocessDate = x.InprocessDate,
                            ResponseUserCode = x.ResponseUserCode,
                            ResponseOrganizationCode = x.ResponseOrganizationCode,
                            TotalCount = x.TotalCount,
                            FailedCount = x.FailedCount,
                            ProcessedCount = x.ProcessedCount,
                            UserCode = x.UserCode,
                            User = x.User,
                            FileSize = x.FileSize,

                            OfficeName = x.OfficeName,

                        }).ToList();
                        var encrypted = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(response));
                        HttpContext.Response.Headers.Append("X-Data-Hash", encrypted.Hash);
                        return Ok(new { data = encrypted.EncryptedData });


                    }
                    else
                    {
                        var err = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                              _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));

                        HttpContext.Response.Headers.Append("X-Data-Hash", err.Hash);
                        return NotFound(new { data = err.EncryptedData });
                    }
                }
                else
                {
                    var err = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));

                    HttpContext.Response.Headers.Append("X-Data-Hash", err.Hash);
                    return BadRequest(new { data = err.EncryptedData });

                }
            }
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                               _encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));

                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);

                return StatusCode(500, new
                {
                    data = data.EncryptedData
                });
            }
        }
        [HttpPost("deleteInternalFile")]
        public async Task<IActionResult> DeletInternalFile([FromBody] EncryptedDataVM Details)
        {
            try
            {

                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                var user = JsonSerializer.Deserialize<StringValueVM>(decryptedData);
                var fileIndb = await _unitofWork.UploadFileInfo.FirstOrDefaultAsync(x => x.FileInfoCode == user.Value, includeProperties: "User");
                if (fileIndb == null)
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, "No File Found")));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return NotFound(new { data = data.EncryptedData });
                }
                else
                {
                    if (fileIndb.UserCode == userInClaim.UserCode)
                    {
                        if (fileIndb.IsInternalOnly == true || fileIndb.Status == "Draft")
                        {

                            var depuser = await _unitofWork.DepartmentUser.FirstOrDefaultAsync(x => x.UserCode == fileIndb.UserCode, includeProperties: "Office");
                            if (depuser != null)
                            {
                                await RemoveStorageAsync(fileIndb.FileSize, depuser);

                            }
                            else
                            {
                                var bankuser = await _unitofWork.BankUser.FirstOrDefaultAsync(x => x.UserCode == fileIndb.UserCode, includeProperties: "BankBranch");
                                if (bankuser != null)
                                {
                                    await RemoveStorageAsync(fileIndb.FileSize, bankuser);

                                }
                            }

                            await _s3Helper.DeleteFileAsync(fileIndb.InternalFilePath);
                            await _unitofWork.UploadFileInfo.RemoveAsync(fileIndb.FileInfoCode);
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Deleted, "File deleted successfully")));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return Ok(new { data = data.EncryptedData });
                        }

                        else
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, "Only internal/Draft File Delete")));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }


                    }
                    else
                    {
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, "Only creator can delete this file")));
                        HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                        return BadRequest(new { data = data.EncryptedData });
                    }
                }
            }
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new { data = data.EncryptedData });
            }
        }
        [HttpPost("sentdraftfile")]
        public async Task<IActionResult> SendDraftFile([FromBody] EncryptedDataVM details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, details.Data);

                var request = JsonSerializer.Deserialize<StringValueVM>(decryptedData);
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                var fileindb = await _unitofWork.UploadFileInfo.FirstOrDefaultAsync(x => x.FileInfoCode == request.Value && x.Status == "Draft", includeProperties: "User");
                var checkhisttory = await _unitofWork.UploadFileInfoHistory.FirstOrDefaultAsync(x => x.FileInfoCode == request.Value);

                if (fileindb == null)
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, "No draft file found")));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return NotFound(new { data = data.EncryptedData });
                }
                else
                {
                    string orgFolder = $"org-{fileindb.OrganizationCode}";
                    string officeFolder = $"office-{fileindb.OfficeCode}";
                    string bankFolder = $"institute-{fileindb.BankBranchCode}";
                    string privacyfolder = "outbox";

                    string sentFolderPath = $"{orgFolder}/{officeFolder}/{bankFolder}/{privacyfolder}";
                    var copyFilePath = await _s3Helper.MoveFileAsync(fileindb.RequestFilePath, sentFolderPath);

                    await _unitofWork.UploadFileInfo.UpdateAsync(fileindb.FileInfoCode, entity =>
                    {
                        entity.Status = "Requested";
                        entity.RequestFilePath = copyFilePath;
                        return Task.CompletedTask;
                    });
                    await _unitofWork.UploadFileInfoHistory.UpdateAsync(checkhisttory.UploadFileInfoHistoryCode, entity =>
                    {
                        entity.ActionDate = DateTime.Now.ToLocalTime();
                        return Task.CompletedTask;
                    });
                }


                var successData = _encryptionHelper.Encrypt(JsonSerializer.Serialize(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Updated, "File sent for approval successfully")));
                HttpContext.Response.Headers.Append("X-Data-Hash", successData.Hash);
                return Ok(new { data = successData.EncryptedData });
            }
            catch (Exception ex)
            {
                var errorData = _encryptionHelper.Encrypt(JsonSerializer.Serialize(_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", errorData.Hash);
                return StatusCode(500, new { data = errorData.EncryptedData });
            }
        }
        [HttpPost("moveuploadedFiledirectory")]
        public async Task<IActionResult> MoveFile([FromBody] EncryptedDataVM details)
        {
            var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, details.Data);
            if (decryptedData != null)
            {
                var move = JsonSerializer.Deserialize<MoveFileVM>(decryptedData);

                var file = await _unitofWork.UploadFileInfo.FirstOrDefaultAsync(f => f.FileInfoCode == move.FileInfoCode);

                if (file != null)
                {
                    if (!string.IsNullOrEmpty(move.TargetDirectoryCode))
                    {
                        var targetFolder = await _unitofWork.UserFileDirectory.FirstOrDefaultAsync(f => f.UserFileDirectoryCode == move.TargetDirectoryCode && !f.IsDeleted);
                        if (targetFolder == null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                                _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, "Target folder not found")));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }
                    }
                    await _unitofWork.UploadFileInfo.UpdateAsync(file.FileInfoCode, async entity =>
                    {
                         entity.UserFileDirectoryCode = move.TargetDirectoryCode;
                        await Task.CompletedTask;
                    });

                    var successData = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Updated, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", successData.Hash);
                    return Ok(new { data = successData.EncryptedData });
                }
                else
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, "Source file not found")));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });
                }
            }
            else
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, "Invalid request")));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return BadRequest(new { data = data.EncryptedData });
            }
        }
        private async Task AddStorageAsync(double fileSize, object user)
        {
            try
            {

                if (user is DepartmentUser dept)
                {
                    await _unitofWork.DepartmentUser.UpdateAsync(dept.DepartmentUserCode, async entity =>
                    {
                        entity.UsedStorageSize += fileSize;
                        await Task.CompletedTask;
                    });

                    await _unitofWork.Office.UpdateAsync(dept.OfficeCode, async entity =>
                    {
                        entity.UsedStorageSize += fileSize;
                        await Task.CompletedTask;
                    });

                    await _unitofWork.Organization.UpdateAsync(dept.Office.OrganizationCode, async entity =>
                    {
                        entity.UsedStorageSize += fileSize;
                        await Task.CompletedTask;
                    });
                }
                else if (user is BankUser bankUser)

                {
                    await _unitofWork.BankUser.UpdateAsync(bankUser.BankUserCode, async entity =>
                    {
                        entity.UsedStorageSize += fileSize;
                        await Task.CompletedTask;
                    });

                    await _unitofWork.BankBranch.UpdateAsync(bankUser.BankBranchCode, async entity =>
                    {
                        entity.UsedStorageSize += fileSize;
                        await Task.CompletedTask;
                    });

                    await _unitofWork.Organization.UpdateAsync(bankUser.BankBranch.OrganizationCode, async entity =>
                    {
                        entity.UsedStorageSize += fileSize;
                        await Task.CompletedTask;
                    });
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

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