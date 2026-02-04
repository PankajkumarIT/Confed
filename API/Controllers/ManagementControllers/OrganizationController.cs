using API.Data.IRepositories;
using API.Helpers.Models;
using API.Model.ManagementModels;
using API.Model.UserModels;
using API.Resources;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using API.Helpers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using API.Model.ViewModels.TransporterManagementViewModels;
using API.Model.ViewModels;
using System.Text;
using NPOI.SS.UserModel;
using NPOI.SS.Formula.Functions;
using NPOI.OpenXml4Net.OPC.Internal;
using System.Security.Cryptography.X509Certificates;

namespace API.Controllers.ManagementControllers
{
    [ApiController]
    [Route(SD.baseUrl + "organization")]
    [Authorize(Policy = SD.IsAccess)]

    public class OrganizationController : ControllerBase
    {
        private readonly IUnitofWork _unitofWork;
        private readonly IEncryptionHelper _encryptionHelper;
        public OrganizationController(IUnitofWork unitofWork, IEncryptionHelper encryptionHelper)
        {
            _unitofWork = unitofWork;
            _encryptionHelper = encryptionHelper;
        }
        [HttpGet("getAllOrganization")]
        public async Task<IActionResult> GetAllOrganization()
        {
            try
            {
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                if (userInClaim.IsEntityUser == true)
                {
                    var organizationclaimcode = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == User.FindFirst(SD.OrganizationCode).Value);

                    var organizationibdb = await _unitofWork.Organization.GetAllAsync(x => x.OrganizationCode == organizationclaimcode.OrganizationCode || x.ParentOrganizationCode == organizationclaimcode.OrganizationCode);
                    var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                  (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, organizationibdb)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                    return Ok(new { data = okdata.EncryptedData });
                }
                else
                {
                    var organizationibdb = await _unitofWork.Organization.GetAllAsync();
                    var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, organizationibdb)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                    return Ok(new { data = okdata.EncryptedData });
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
        [HttpGet("getAllDepartmentOrganization")]
        public async Task<IActionResult> GetAllDepartment()
        {
            try
            {
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                if (userInClaim.IsEntityUser == true)
                {
                    var organizationclaimcode = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == User.FindFirst(SD.OrganizationCode).Value);

                    var organizationibdb = await _unitofWork.Organization.GetAllAsync(x => x.OrganizationCode == organizationclaimcode.OrganizationCode);
                    var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                  (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Sent, organizationibdb)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                    return Ok(new { data = okdata.EncryptedData });
                }
                else
                {
                    var organizationibdb = await _unitofWork.Organization.GetAllAsync(x => x.OrganizationType == "department");
                    var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                  (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Sent, organizationibdb)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                    return Ok(new { data = okdata.EncryptedData });
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
        [HttpGet("getTransportOrganization")]
        public async Task<IActionResult> GetTransportOrganization()
        {
            try
            {
                var userInClaim = await _unitofWork.User
                    .FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                if (userInClaim.IsEntityUser == true)
                {
                    var organizationclaimcode = await _unitofWork.Organization
                        .FirstOrDefaultAsync(x => x.OrganizationCode == User.FindFirst(SD.OrganizationCode).Value);

                    var organizationibdb = await _unitofWork.Organization.GetAllAsync(
                        x =>
                            (x.OrganizationCode == organizationclaimcode.OrganizationCode && x.OrganizationType == "transporter")
                            ||
                            (x.ParentOrganizationCode == organizationclaimcode.OrganizationCode && x.OrganizationType == "transporter")
                    );

                    var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                    (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, organizationibdb)));

                    HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                    return Ok(new { data = okdata.EncryptedData });
                }
                else
                {
                    var organizationibdb = await _unitofWork.Organization
                        .GetAllAsync(x => x.OrganizationType == "transporter");
                    var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                    (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, organizationibdb)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                    return Ok(new { data = okdata.EncryptedData });
                }
            }
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new { data = data.EncryptedData });
            }
        }

        [HttpGet("getBankOrganization")]
        public async Task<IActionResult> GetBankOrganization()
        {
            try
            {
                var userInClaim = await _unitofWork.User
                    .FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                if (userInClaim.IsEntityUser == true)
                {
                    var organizationclaimcode = await _unitofWork.Organization
                        .FirstOrDefaultAsync(x => x.OrganizationCode == User.FindFirst(SD.OrganizationCode).Value);

                    var organizationibdb = await _unitofWork.Organization.GetAllAsync(
                        x =>
                            (x.OrganizationCode == organizationclaimcode.OrganizationCode && x.OrganizationType == "bank")
                            ||
                            (x.ParentOrganizationCode == organizationclaimcode.OrganizationCode && x.OrganizationType == "bank")
                    );

                    var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                    (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, organizationibdb)));

                    HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                    return Ok(new { data = okdata.EncryptedData });
                }
                else
                {
                    var organizationibdb = await _unitofWork.Organization
                        .GetAllAsync(x => x.OrganizationType == "bank");

                    var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                    (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, organizationibdb)));

                    HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                    return Ok(new { data = okdata.EncryptedData });
                }
            }
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                               (_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));

                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new { data = data.EncryptedData });
            }
        }
        [HttpPost("addOrganization")]
        public async Task<IActionResult> AddOrganization([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData == null)
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)
                    ));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });
                }

                RegisterOrganizationVM user = JsonSerializer.Deserialize<RegisterOrganizationVM>(decryptedData);

                if (!TryValidateModel(user))
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest,
                            ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList())
                    ));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });
                }

                var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                if (userRoleInClaim.RoleLevel != RoleLevels.SUPREME && userRoleInClaim.RoleLevel != RoleLevels.ADMIN)
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)
                    ));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });
                }

                var organizationInDb = await _unitofWork.Organization.FirstOrDefaultAsync(x =>
                    x.OrganizationName == user.OrganizationName || x.OrganizationCode == user.OrganizationCode);

                if (organizationInDb != null)
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, null)
                    ));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return NotFound(new { data = data.EncryptedData });
                }
                string paymentFileHeader = null;
                string paymentResponseHeader = null;
                string paymentAckHeader = null;
                string paymentNotAckHeader = null;

             
                if (user.OrganizationType == "bank" &&
                    (user.FetureType == "SFTPForPaymentTransaction" || user.FetureType == "SFTPForPaymentWithEncryption"))
                {
                    string expectedHeader = "transactionNo,bankname,branchname,ifsc";

                    if (!string.IsNullOrEmpty(user.PaymentFileHeader))
                    {
                        var file = _unitofWork.Organization.DecrypteIFromFileBase64(user.PaymentFileHeader);
                        var result = _unitofWork.UploadFileInfo.ReadFile(file);
                        _unitofWork.UploadFileInfo.ValidateHeaders(result.fileHeader, expectedHeader);

                        paymentFileHeader = result.fileHeader;
                    }

                    if (!string.IsNullOrEmpty(user.PaymentResponseFileHeader))
                    {
                        var file = _unitofWork.Organization.DecrypteIFromFileBase64(user.PaymentResponseFileHeader);
                        var result = _unitofWork.UploadFileInfo.ReadFile(file);
                        _unitofWork.UploadFileInfo.ValidateHeaders(result.fileHeader, expectedHeader);

                        paymentResponseHeader = result.fileHeader;
                    }

                    if (!string.IsNullOrEmpty(user.PaymentAcknowledgment))
                    {
                        var file = _unitofWork.Organization.DecrypteIFromFileBase64(user.PaymentAcknowledgment);
                        var result = _unitofWork.UploadFileInfo.ReadFile(file);
                        _unitofWork.UploadFileInfo.ValidateHeaders(result.fileHeader, expectedHeader);

                        paymentAckHeader = result.fileHeader;
                    }

                    if (!string.IsNullOrEmpty(user.PaymentNotAcknowledgement))
                    {
                        var file = _unitofWork.Organization.DecrypteIFromFileBase64(user.PaymentNotAcknowledgement);
                        var result = _unitofWork.UploadFileInfo.ReadFile(file);
                        _unitofWork.UploadFileInfo.ValidateHeaders(result.fileHeader, expectedHeader);

                        paymentNotAckHeader = result.fileHeader;
                    }
                }
                Organization organization = new Organization()
                {
                    OrganizationCode = _unitofWork.Organization.GenrateUniqueCode(),
                    OrganizationName = user.OrganizationName,
                    Address = user.Address,
                    ContactNumber = user.ContactNumber,
                    Email = user.Email,
                    OrganizationType = user.OrganizationType,
                    ParentOrganizationCode = user.ParentOrganizationCode,
                    FetureType = user.FetureType,
                    StorageSize = user.StorageSize,
                    PaymentFileHeader = paymentFileHeader,
                    PaymentResponseFileHeader = paymentResponseHeader,
                    PaymentAcknowledgment = paymentAckHeader,
                    ShortName = user.ShortName,
                    PaymentNotAcknowledgement = paymentNotAckHeader,
                    IsActive = true,
                    PanNumber =user.PanNumber,
                    GstNumber = user.GstNumber,
                    IsConversion=user.IsConversion,
                };

                await _unitofWork.Organization.AddAsync(organization);
                var okData = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Created, user)
                ));
                HttpContext.Response.Headers.Append("X-Data-Hash", okData.Hash);
                return Ok(new { data = okData.EncryptedData });

            }
            catch (Exception ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, ex.Message)
                ));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new { data = data.EncryptedData });
            }
        }
        [HttpPost("addDepartmentOrganization")]
        public async Task<IActionResult> AddDepartmentOrganization([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    RegisterDepartmentOrganizationVM user = JsonSerializer.Deserialize<RegisterDepartmentOrganizationVM>(decryptedData);
                    if (TryValidateModel(user))
                    {
                        var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                        var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                        if (userRoleInClaim.RoleLevel != RoleLevels.SUPREME && userRoleInClaim.RoleLevel != RoleLevels.ADMIN)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }


                        var Organizationindb = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationName == user.OrganizationName || x.OrganizationCode == user.OrganizationCode);
                        if (Organizationindb != null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return Ok(new { data = data.EncryptedData });
                        }
                        else
                        {
                            var fileHeader = "";
                            var outputHeader = "";
                            string expectedHeader = "transactionNo,bankname,branchname,ifsc";

                            if (!string.IsNullOrEmpty(user.FileHeader))
                            {
                                var file = _unitofWork.Organization.DecrypteIFromFileBase64(user.FileHeader);
                                var result = _unitofWork.UploadFileInfo.ReadFile(file);
                                _unitofWork.UploadFileInfo.ValidateHeaders(result.fileHeader, expectedHeader);

                                fileHeader = result.fileHeader;
                            }

                            if (!string.IsNullOrEmpty(user.OutputFileHeader))
                            {
                                var file = _unitofWork.Organization.DecrypteIFromFileBase64(user.OutputFileHeader);
                                var result = _unitofWork.UploadFileInfo.ReadFile(file);
                                _unitofWork.UploadFileInfo.ValidateHeaders(result.fileHeader, expectedHeader);

                                outputHeader = result.fileHeader;
                            }

                            Organization organization = new Organization()
                            {
                                OrganizationCode = _unitofWork.Organization.GenrateUniqueCode(),
                                OrganizationName = user.OrganizationName,
                                Address = user.Address,
                                ContactNumber = user.ContactNumber,
                                Email = user.Email,
                                OrganizationType = "department",
                                FetureType = user.FetureType,
                                StorageSize = user.StorageSize,
                                ParentOrganizationCode = user.ParentOrganizationCode,
                                ShortName=user.ShortName,
                                IsActive = true
                            };
                            await _unitofWork.Organization.AddAsync(organization);
                        }


                        var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Created, user)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                        return Ok(new { data = okdata.EncryptedData });

                    }

                    else
                    {
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                            (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList())));
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

        [HttpPost("addBankOrganization")]
        public async Task<IActionResult> AddBankOrganization([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    RegisterBankOrganizationVM user = JsonSerializer.Deserialize<RegisterBankOrganizationVM>(decryptedData);
                    if (TryValidateModel(user))
                    {
                        var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                        var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                        if (userRoleInClaim.RoleLevel != RoleLevels.SUPREME && userRoleInClaim.RoleLevel != RoleLevels.ADMIN)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }


                        var Organizationindb = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationName == user.OrganizationName || x.OrganizationCode == user.OrganizationCode);
                        if (Organizationindb != null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return Ok(new { data = data.EncryptedData });
                        }
                        else
                        {
                            string expectedHeader = "transactionNo,bankname,branchname,ifsc";

                            string paymentFileHeader = null;
                            string paymentResponseHeader = null;
                            string paymentAckHeader = null;
                            string paymentNotAckHeader = null;
                            if (!string.IsNullOrEmpty(user.PaymentFileHeader))
                            {
                                var file = _unitofWork.Organization.DecrypteIFromFileBase64(user.PaymentFileHeader);
                                var result = _unitofWork.UploadFileInfo.ReadFile(file);
                                _unitofWork.UploadFileInfo.ValidateHeaders(result.fileHeader, expectedHeader);

                                paymentFileHeader = result.fileHeader;
                            }

                            if (!string.IsNullOrEmpty(user.PaymentResponseFileHeader))
                            {
                                var file = _unitofWork.Organization.DecrypteIFromFileBase64(user.PaymentResponseFileHeader);
                                var result = _unitofWork.UploadFileInfo.ReadFile(file); 
                                _unitofWork.UploadFileInfo.ValidateHeaders(result.fileHeader, expectedHeader);

                                paymentResponseHeader = result.fileHeader;
                            }

                            if (!string.IsNullOrEmpty(user.PaymentAcknowledgment))
                            {
                                var file = _unitofWork.Organization.DecrypteIFromFileBase64(user.PaymentAcknowledgment);
                                var result = _unitofWork.UploadFileInfo.ReadFile(file);
                                _unitofWork.UploadFileInfo.ValidateRequiredFieldConsistency(result.rows, user.PaymentAcknowledgment.Split(',').ToList(), null);
                                paymentAckHeader = result.fileHeader;
                            }

                            if (!string.IsNullOrEmpty(user.PaymentNotAcknowledgement))
                            {
                                var file = _unitofWork.Organization.DecrypteIFromFileBase64(user.PaymentNotAcknowledgement);
                                var result = _unitofWork.UploadFileInfo.ReadFile(file); _unitofWork.UploadFileInfo.ValidateHeaders(result.fileHeader, expectedHeader);

                                paymentNotAckHeader = result.fileHeader;
                            }

                            Organization organization = new Organization()
                            {
                                OrganizationCode = _unitofWork.Organization.GenrateUniqueCode(),
                                OrganizationName = user.OrganizationName,
                                Address = user.Address,
                                ContactNumber = user.ContactNumber,
                                Email = user.Email,
                                OrganizationType = "bank",
                                ParentOrganizationCode = user.ParentOrganizationCode,
                                StorageSize = user.StorageSize,
                                FetureType = user.FetureType,
                                PaymentFileHeader = paymentFileHeader,
                                PaymentResponseFileHeader = paymentResponseHeader,
                                PaymentAcknowledgment = paymentAckHeader,
                                PaymentNotAcknowledgement = paymentNotAckHeader,
                                ShortName=user.ShortName,
                                IsActive = true
                            };
                            await _unitofWork.Organization.AddAsync(organization);
                        }


                        var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Created, user)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                        return Ok(new { data = okdata.EncryptedData });

                    }

                    else
                    {
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                            (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList())));
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
        [HttpPost("addtransportOrganization")]
        public async Task<IActionResult> AddtransportOrganization([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    RegisterTransportOrganizationVM user = JsonSerializer.Deserialize<RegisterTransportOrganizationVM>(decryptedData);
                    if (TryValidateModel(user))
                    {
                        var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                        var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                        if (userRoleInClaim.RoleLevel != RoleLevels.SUPREME && userRoleInClaim.RoleLevel != RoleLevels.ADMIN && userRoleInClaim.RoleLevel != RoleLevels.AUTHORITY)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                        }


                        var Organizationindb = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationName == user.OrganizationName || x.OrganizationCode == user.OrganizationCode);
                        if (Organizationindb != null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return NotFound(new { data = data.EncryptedData });
                        }
                        else
                        {

                            Organization organization = new Organization()
                            {
                                OrganizationCode = _unitofWork.Organization.GenrateUniqueCode(),
                                OrganizationName = user.OrganizationName,
                                Address = user.Address,
                                ContactNumber = user.ContactNumber,
                                Email = user.Email,
                                OrganizationType = "transporter",
                                ParentOrganizationCode = user.ParentOrganizationCode,
                                IsActive = true,
                                PanNumber =user.PanNumber,
                                ShortName=user.ShortName,
                                GstNumber=user.GstNumber,
                            };
                            await _unitofWork.Organization.AddAsync(organization);
                        }


                        var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Created, user)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                        return Ok(new { data = okdata.EncryptedData });

                    }

                    else
                    {
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                            (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList())));
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

        [HttpPost("updateOrganzation")]
        public async Task<IActionResult> UpdateOrganzation([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    Organization user = JsonSerializer.Deserialize<Organization>(decryptedData);
                    if (TryValidateModel(user))
                    {
                        var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                        var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                        if (userRoleInClaim.RoleLevel != RoleLevels.SUPREME && userRoleInClaim.RoleLevel != RoleLevels.ADMIN)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }


                        var Organizationindb = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == user.OrganizationCode);
                        if (Organizationindb == null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return Ok(new { data = data.EncryptedData });
                        }
                        else
                        {
                            await _unitofWork.Organization.UpdateAsync(Organizationindb.OrganizationCode, async entity =>
                            {
                                entity.OrganizationName = user.OrganizationName;
                                entity.Address = user.Address;
                                entity.ContactNumber = user.ContactNumber;
                                entity.Email = user.Email;
                                entity.ParentOrganizationCode = user.ParentOrganizationCode;
                                entity.OrganizationType = user.OrganizationType;
                                entity.PanNumber = user.PanNumber;
                                entity.GstNumber = user.GstNumber;
                                entity.ShortName = user.ShortName;
                                entity.StorageSize = user.StorageSize;
                                entity.IsActive = true;
                                await Task.CompletedTask;
                            });
                        }
                        var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Created, user)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                        return Ok(new { data = okdata.EncryptedData });
                    }

                    else
                    {
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                            (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList())));
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
        [HttpPost("updateDepartmentorganzation")]
        public async Task<IActionResult> UpdateDepartmentOrganzation([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    Organization user = JsonSerializer.Deserialize<Organization>(decryptedData);
                    if (TryValidateModel(user))
                    {
                        var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                        var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                        if (userRoleInClaim.RoleLevel != RoleLevels.SUPREME && userRoleInClaim.RoleLevel != RoleLevels.ADMIN)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }


                        var Organizationindb = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == user.OrganizationCode);
                        if (Organizationindb == null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return Ok(new { data = data.EncryptedData });
                        }
                        else
                        {
                           
                            await _unitofWork.Organization.UpdateAsync(Organizationindb.OrganizationCode, async entity =>
                            {
                                entity.OrganizationName = user.OrganizationName;
                                entity.Address = user.Address;
                                entity.ContactNumber = user.ContactNumber;
                                entity.Email = user.Email;
                                entity.ShortName = user.ShortName;

                                entity.ParentOrganizationCode = user.ParentOrganizationCode;
                                entity.IsActive = true;
                                await Task.CompletedTask;
                            });
                        }
                        var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Created, user)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                        return Ok(new { data = okdata.EncryptedData });
                    }

                    else
                    {
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                            (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList())));
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

        [HttpPost("updateBankorganzation")]
        public async Task<IActionResult> UpdateBankOrganzation([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    Organization user = JsonSerializer.Deserialize<Organization>(decryptedData);
                    if (TryValidateModel(user))
                    {
                        var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                        var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                        if (userRoleInClaim.RoleLevel != RoleLevels.SUPREME && userRoleInClaim.RoleLevel != RoleLevels.ADMIN)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }


                        var Organizationindb = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == user.OrganizationCode);
                        if (Organizationindb == null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return Ok(new { data = data.EncryptedData });
                        }
                        else
                        {
                            await _unitofWork.Organization.UpdateAsync(Organizationindb.OrganizationCode, async entity =>
                            {
                                entity.OrganizationName = user.OrganizationName;
                                entity.Address = user.Address;
                                entity.ContactNumber = user.ContactNumber;
                                entity.Email = user.Email;
                                entity.ShortName = user.ShortName;

                                entity.ParentOrganizationCode = user.ParentOrganizationCode;
                                entity.IsActive = true;
                                await Task.CompletedTask;

                            });
                        }


                        var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Created, user)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                        return Ok(new { data = okdata.EncryptedData });

                    }

                    else
                    {
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                            (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList())));
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
        [HttpPost("updateTransportorganzation")]
        public async Task<IActionResult> UpdateTransportOrganzation([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    Organization user = JsonSerializer.Deserialize<Organization>(decryptedData);
                    if (TryValidateModel(user))
                    {
                        var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                        var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                        if (userRoleInClaim.RoleLevel != RoleLevels.SUPREME && userRoleInClaim.RoleLevel != RoleLevels.ADMIN && userRoleInClaim.RoleLevel != RoleLevels.AUTHORITY)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return Ok(new { data = data.EncryptedData });
                        }


                        var Organizationindb = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == user.OrganizationCode);
                        if (Organizationindb == null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return Ok(new { data = data.EncryptedData });
                        }
                        else
                        {
                           
                            await _unitofWork.Organization.UpdateAsync(Organizationindb.OrganizationCode, async entity =>
                            {
                                entity.OrganizationName = user.OrganizationName;
                                entity.Address = user.Address;
                                entity.ContactNumber = user.ContactNumber;
                                entity.Email = user.Email;
                                entity.ParentOrganizationCode = user.ParentOrganizationCode;
                                entity.PanNumber = user.PanNumber;
                                entity.ShortName = user.ShortName;

                                entity.GstNumber = user.GstNumber;
                                entity.IsActive = true;
                                await Task.CompletedTask;

                            });
                        }


                        var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Created, user)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                        return Ok(new { data = okdata.EncryptedData });

                    }

                    else
                    {
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                            (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList())));
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
        [HttpPost("getByorganizationcode")]
        public async Task<IActionResult> GetByOrganizationcode([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    StringValueVM user = JsonSerializer.Deserialize<StringValueVM>(decryptedData);
                    if (TryValidateModel(user))
                    {
                        var vehicletypeindb = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == user.Value);
                        if (vehicletypeindb == null)
                        {
                            var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                           (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                            return NotFound(new { data = okdata.EncryptedData });
                        }
                        else
                        {
                            var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                           (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, vehicletypeindb)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                            return Ok(new { data = okdata.EncryptedData });

                        }

                    }

                    else
                    {
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                            (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList())));
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
        [HttpPost("getByParentorganizationcode")]
        public async Task<IActionResult> GetByParentorganizationcode([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    StringValueVM user = JsonSerializer.Deserialize<StringValueVM>(decryptedData);
                    if (TryValidateModel(user))
                    {
                        var vehicletypeindb = await _unitofWork.Organization.GetAllAsync(x => x.ParentOrganizationCode == user.Value);
                        if (vehicletypeindb == null)
                        {
                            var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                           (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                            return NotFound(new { data = okdata.EncryptedData });
                        }
                        else
                        {
                            var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                           (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, vehicletypeindb)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                            return Ok(new { data = okdata.EncryptedData });

                        }

                    }

                    else
                    {
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                            (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList())));
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
        [HttpPost("getByOrganizationType")]
        public async Task<IActionResult> GetByOrganizationType([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                if (decryptedData != null)
                {
                    StringValueVM user = JsonSerializer.Deserialize<StringValueVM>(decryptedData);

                    if (TryValidateModel(user))
                    {
                        if (userInClaim.IsEntityUser == true)
                        {
                            var organizationclaimcode = await _unitofWork.Organization
                                .FirstOrDefaultAsync(x =>
                                    x.OrganizationCode == User.FindFirst(SD.OrganizationCode).Value);

                            var orgaisxationinb = await _unitofWork.Organization.GetAllAsync(x =>
                                (x.OrganizationCode == organizationclaimcode.OrganizationCode && x.OrganizationType == user.Value)
                                ||
                                (x.ParentOrganizationCode == organizationclaimcode.OrganizationCode && x.OrganizationType == user.Value)
                            );

                            var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, orgaisxationinb)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                            return Ok(new { data = okdata.EncryptedData });
                        }
                        else
                        {
                            var orgaisxationinb = await _unitofWork.Organization.GetAllAsync(x => x.OrganizationType == user.Value);

                            var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, orgaisxationinb)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                            return Ok(new { data = okdata.EncryptedData });
                        }
                    }
                    else
                    {
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                           (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest,
                            ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList())));
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

                return StatusCode(500, new { data = data.EncryptedData });
            }
        }
        [HttpPost("getTansportOrganizationbycode")]
        public async Task<IActionResult> GetTansportOrganizationbycode([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    StringValueVM user = JsonSerializer.Deserialize<StringValueVM>(decryptedData);
                    if (TryValidateModel(user))
                    {
                        var tranportorganization = await _unitofWork.Organization.GetAllAsync(x => x.ParentOrganizationCode == user.Value && x.OrganizationType == "transport");
                        if (tranportorganization == null)
                        {
                            var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                           (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                            return NotFound(new { data = okdata.EncryptedData });
                        }
                        else
                        {
                            var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                           (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, tranportorganization)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                            return Ok(new { data = okdata.EncryptedData });

                        }

                    }

                    else
                    {
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                            (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList())));
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
        [HttpPost("departmentbankOrganizationMap")]
        public async Task<IActionResult> DepartmentbankOrganizationMap([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    DepartmentBankOrganizationMap departmentorg = JsonSerializer.Deserialize<DepartmentBankOrganizationMap>(decryptedData);

                    if (departmentorg != null)
                    {
                        
                            var InDb = await _unitofWork.DepartmentBankOrganizationsMap.FirstOrDefaultAsync(x => x.DepartmentOrganizationCode == departmentorg.DepartmentOrganizationCode &&
                            x.BankOrganizationCode == departmentorg.BankOrganizationCode);

                            string fileHeader = null;
                            string expectedHeader = "transactionNo,bankname,branchname,ifsc";

                            if (InDb == null)
                            {
                                if (!string.IsNullOrEmpty(departmentorg.InputFileHeader))
                                {
                                    var file = _unitofWork.Organization.DecrypteIFromFileBase64(departmentorg.InputFileHeader);
                                    var result = _unitofWork.UploadFileInfo.ReadFile(file);
                                    _unitofWork.UploadFileInfo.ValidateHeaders(result.fileHeader, expectedHeader);
                                    fileHeader = result.fileHeader;
                                }
                                DepartmentBankOrganizationMap mapping = new DepartmentBankOrganizationMap()
                                {
                                    DepartmentBankOrganizationMapCode = _unitofWork.DepartmentBankOrganizationsMap.GenrateUniqueCode(),
                                    DepartmentOrganizationCode = departmentorg.DepartmentOrganizationCode,
                                    BankOrganizationCode = departmentorg.BankOrganizationCode,
                                    InputFileHeader = fileHeader,
                                    IsMapped = true
                                };
                                await _unitofWork.DepartmentBankOrganizationsMap.AddAsync(mapping);
                            }
                            else if (InDb.IsMapped != departmentorg.IsMapped)
                            {
                                await _unitofWork.DepartmentBankOrganizationsMap.UpdateAsync(InDb.DepartmentBankOrganizationMapCode, async entity =>
                                {
                                    entity.IsMapped = departmentorg.IsMapped;
                                    await Task.CompletedTask;
                                });
                            }
                        
                        var responseData = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, ValidationMessages.Ok)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", responseData.Hash);
                        return Ok(new { data = responseData.EncryptedData });
                    }
                    else
                    {
                        var badData = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, "Invalid or empty data")));
                        HttpContext.Response.Headers.Append("X-Data-Hash", badData.Hash);
                        return BadRequest(new { data = badData.EncryptedData });
                    }
                }
                else
                {
                    var badData = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", badData.Hash);
                    return BadRequest(new { data = badData.EncryptedData });
                }
            }
            catch (Exception ex)
            {
                var errorData = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                _encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", errorData.Hash);
                return StatusCode(500, new { data = errorData.EncryptedData });
            }


        }
        [HttpPost("getAllDepartmentBankMapOrganization")]
        public async Task<IActionResult> GetAllDepartmentBankMapOrganization([FromBody] EncryptedDataVM Details)
        
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    StringValueVM request = JsonSerializer.Deserialize<StringValueVM>(decryptedData);
                    var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                    var organizationibdb = await _unitofWork.DepartmentBankOrganizationsMap.GetAllAsync(x => x.DepartmentOrganizationCode == request.Value, includeProperties: "DepartmentOrganization,BankOrganization");
                    var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                    (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Sent, organizationibdb)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                    return Ok(new { data = okdata.EncryptedData });
                }
                else
                {
                    var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                    (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                    return BadRequest(new { data = okdata.EncryptedData });
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
    }
}
