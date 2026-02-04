using API.Data.IRepositories;
using API.Data.Repositories;
using API.Helpers;
using API.Helpers.Models;
using API.Model.ManagementModels.UserModels;
using API.Model.ManagementModels;
using API.Model.UserModels;
using API.Model.ViewModels;
using API.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using API.Model.ViewModels.TransporterManagementViewModels;

namespace API.Controllers.ManagementControllers
{
    [ApiController]

    [Route(SD.baseUrl + "adminstoruser")]
    [Authorize(Policy = SD.IsAccess)]

    public class AdministorUserController : ControllerBase
    {

        private readonly IUnitofWork _unitofWork;
        private readonly IEncryptionHelper _encryptionHelper;
        public AdministorUserController(IUnitofWork unitofWork, IEncryptionHelper encryptionHelper)
        {
            _unitofWork = unitofWork;
            _encryptionHelper = encryptionHelper;
        }

        [HttpGet("getAllAdministorUser")]
        public async Task<IActionResult> GetAllAdministorUser()
        {
            try
            {
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                var organizationibdb = await _unitofWork.AdministorUser.GetAllAsync(includeProperties: "Organization,User.Ctv.District.State");
                var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>( _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, organizationibdb)));
                HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                return Ok(new { data = okdata.EncryptedData });
                
            }
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new { data = data.EncryptedData });
            }
        }


        [HttpPost("getAdminstorByOrganizationCode")]
        public async Task<IActionResult> GetAllAdminstorUser([FromBody] EncryptedDataVM details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, details.Data);
                if (decryptedData == null)
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });
                }
                StringValueVM orgcode = JsonSerializer.Deserialize<StringValueVM>(decryptedData);
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                var organizationibdb = await _unitofWork.AdministorUser.GetAllAsync(x => x.OrganizationCode == orgcode.Value,includeProperties: "Organization,User.Ctv.District.State");
                var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, organizationibdb)));

                HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                return Ok(new { data = okdata.EncryptedData });
            }
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));

                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new { data = data.EncryptedData });
            }
        }

       [HttpPost("registerAdministorUser")]
        public async Task<IActionResult> RegisterAdminstorUser([FromBody] EncryptedDataVM Details)
            {
                try
                {
                    var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                    if (decryptedData != null)
                    {
                        RegisterAdministorVM user = JsonSerializer.Deserialize<RegisterAdministorVM>(decryptedData);
                        if (TryValidateModel(user))
                        {
                            var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                            var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                            var getuserOrganization = await _unitofWork.Organization
                                .FirstOrDefaultAsync(x => x.OrganizationCode == user.OrganizationCode);
                            UserRole requiredRole = null;

                            if (getuserOrganization.OrganizationType == "department")
                            {
                                requiredRole = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleLevel == RoleLevels.AUTHORITY && d.RoleType == "department");
                            }
                            else if (getuserOrganization.OrganizationType == "transporter")
                            {
                                requiredRole = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleLevel == RoleLevels.AUTHORITY && d.RoleType == "transporter");
                            }
                            else
                            {
                                requiredRole = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleLevel == RoleLevels.AUTHORITY && d.RoleType == "bank");
                            }
                            if (requiredRole == null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                 (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return NotFound(new { data = data.EncryptedData });
                        }
                            var IsuniqueUser = await _unitofWork.User.IsUniqueUser(user.MobileNumber);
                            if (!IsuniqueUser)
                            {
                                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                  (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, null)));
                                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                                return BadRequest(new { data = data.EncryptedData });
                            }
                            else
                            {
                                User adduser = new User()
                                {
                                    UserCode = _unitofWork.User.GenrateUniqueCode(),
                                    Name = user.Name,
                                    Password = user.Password,
                                    MobileNumber = user.MobileNumber,
                                    EMail = user.EMail,
                                    Address = user.Address,
                                    CtvCode = user.CtvCode,
                                    IsActive = true,
                                    IsEntityUser = true,
                                    CreatedBy = userInClaim.Name + "/" + userInClaim.UserCode,
                                    CreatedOn = DateTime.Now.ToLocalTime()
                                };
                                RoleAccess userrole = new RoleAccess
                                {
                                    AccessId = _unitofWork.RoleAccess.GenrateUniqueCode(),
                                    UserCode = adduser.UserCode,
                                    RoleCode = requiredRole.RoleCode,
                                    AccessToRole = true
                                };
                                Administor entitydetail = new Administor()
                                {
                                    AdministorCode = _unitofWork.AdministorUser.GenrateUniqueCode(),
                                    OrganizationCode = user.OrganizationCode,
                                    UserCode = adduser.UserCode,
                                    RoleCode = requiredRole.RoleCode,
                                };
                                await _unitofWork.User.RegisterUser(adduser);
                                await _unitofWork.RoleAccess.AddAsync(userrole);
                                await _unitofWork.AdministorUser.AddAsync(entitydetail);
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

        [HttpPost("activedeactiveuser")]
        [Authorize(Policy = SD.IsAccess)]
        public async Task<IActionResult> ActiveDeactiveUser([FromBody] EncryptedDataVM details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, details.Data);
                if (decryptedData == null)
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });
                }

                StringValueVM requestData = JsonSerializer.Deserialize<StringValueVM>(decryptedData);

                if (string.IsNullOrEmpty(requestData.Value))
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });
                }
                var roleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                var indb = await _unitofWork.User.FirstOrDefaultAsync(d => d.UserCode == requestData.Value);

                if (indb == null)
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return NotFound(new { data = data.EncryptedData });
                }

                var inDbRoles = (await _unitofWork.RoleAccess.GetAllAsync(ra => ra.UserCode == indb.UserCode,includeProperties: "User,UserRole")).ToList();
                await _unitofWork.User.ActiveDeactiveUser(indb.UserCode);

                var updatedUser = await _unitofWork.User.FirstOrDefaultAsync(d => d.UserCode == indb.UserCode);
                var userRoles = (await _unitofWork.RoleAccess.GetAllAsync(ra => ra.UserCode == updatedUser.UserCode, includeProperties: "UserRole")).OrderByDescending(x => x.UserRole.RoleLevel).ToList();
                if (!updatedUser.IsActive)
                {
                    foreach (var userRole in userRoles)
                    {
                        await _unitofWork.RoleAccess.UpdateAsync(userRole.AccessId, async entity =>
                        {
                            entity.AccessToRole = false;
                            await Task.CompletedTask;
                        });
                    }
                }
                else
                {
                    await _unitofWork.RoleAccess.UpdateAsync(userRoles.First().AccessId, async entity =>
                    {
                        entity.AccessToRole = true;
                        await Task.CompletedTask;
                    });
                }

                var successData = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Updated, null)));
                HttpContext.Response.Headers.Append("X-Data-Hash", successData.Hash);
                return Ok(new { data = successData.EncryptedData });
            }
            catch (Exception ex)
            {
                var errorData = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ValidationMessages.Default, ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", errorData.Hash);
                return StatusCode(500, new { data = errorData.EncryptedData });
            }
        }
        [HttpPost("updateAdministoruser")]
        [Authorize(Policy = SD.IsAccess)]
        public async Task<IActionResult> UpdateUser([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);
                if (decryptedData == null)
                {
                    var resp = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", resp.Hash);
                    return BadRequest(new { data = resp.EncryptedData });
                }
                RegisterAdministorVM user = JsonSerializer.Deserialize<RegisterAdministorVM>(decryptedData);

                if (!TryValidateModel(user))
                {
                    var resp = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest,ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList())));
                    HttpContext.Response.Headers.Append("X-Data-Hash", resp.Hash);
                    return BadRequest(new { data = resp.EncryptedData });
                }
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(d => d.UserCode == User.FindFirst(ClaimTypes.SerialNumber).Value);
                var getadmin = await _unitofWork.AdministorUser.FirstOrDefaultAsync(x => x.AdministorCode == user.AdministorCode, includeProperties: "User");
                var roleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                if (getadmin.UserCode != userInClaim.UserCode)
                {
                    var indb = await _unitofWork.User.FirstOrDefaultAsync(d => d.UserCode == getadmin.UserCode);
                    if (indb == null)
                    {
                        var resp = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, ValidationMessages.NotFound)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", resp.Hash);
                        return NotFound(new { data = resp.EncryptedData });
                    }
                    var inDbRoles = (await _unitofWork.RoleAccess.GetAllAsync( ra => ra.UserCode == indb.UserCode,includeProperties: "User,UserRole")).ToList();

                    if (roleInClaim.RoleLevel != RoleLevels.SUPREME && roleInClaim.RoleLevel != RoleLevels.AUTHORITY && inDbRoles.Any(r => r.UserRole.RoleLevel >= roleInClaim.RoleLevel))
                    {
                        var resp = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NoAccess, ValidationMessages.NoAccess)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", resp.Hash);
                        return BadRequest(new { data = resp.EncryptedData });
                    }
                    await _unitofWork.User.UpdateAsync(getadmin.UserCode, async entity =>
                    {
                        entity.Name = user.Name;
                        entity.MobileNumber = user.MobileNumber;
                        entity.EMail = user.EMail;
                        entity.Address = user.Address;
                        entity.CtvCode = user.CtvCode;
                        entity.UpdatedBy = userInClaim.Name + "/" + userInClaim.UserCode;
                        entity.UpdatedOn = DateTime.Now.ToLocalTime();
                        await Task.CompletedTask;
                    });                 
                }
                else
                {
                    await _unitofWork.User.UpdateAsync(getadmin.UserCode, async entity =>
                    {
                        entity.Name = user.Name;
                        entity.EMail = user.EMail;
                        entity.Address = user.Address;
                        entity.CtvCode = user.CtvCode;
                        entity.UpdatedBy = userInClaim.Name + "/" + userInClaim.UserCode;
                        entity.UpdatedOn = DateTime.Now.ToLocalTime();
                        await Task.CompletedTask;
                    });
                }
                var okresp = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Updated, ValidationMessages.Updated)));
                HttpContext.Response.Headers.Append("X-Data-Hash", okresp.Hash);
                return Ok(new { data = okresp.EncryptedData });
            }
            catch (Exception Ex)
            {
                var resp = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", resp.Hash);
                return StatusCode(500, new { data = resp.EncryptedData });
            }
        }
    }
}
