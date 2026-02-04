using API.Data.IRepositories;
using API.Helpers;
using API.Helpers.Models;
using API.Model.ManagementModels.DepartmentManagement;
using API.Model.ManagementModels.UserModels;
using API.Model.UserModels;
using API.Model.ViewModels;
using API.Model.ViewModels.TransporterManagementViewModels;
using API.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPOI.HPSF;
using NPOI.SS.Formula.Functions;
using System.Security.Claims;
using System.Text.Json;

namespace API.Controllers.DepartmentControllers
{
    [ApiController]
    [Route(SD.baseUrl + "departmentuser")]
    [Authorize(Policy = SD.IsAccess)]
     public class DepartmentUserController : ControllerBase
    {
        private readonly IUnitofWork _unitofWork;
        private readonly IEncryptionHelper _encryptionHelper;
        public DepartmentUserController(IUnitofWork unitofWork, IEncryptionHelper encryptionHelper)
        {
            _unitofWork = unitofWork;
            _encryptionHelper = encryptionHelper;
        }

        [HttpGet("getAllDepartmentUser")]
        public async Task<IActionResult> GetAllDepartmentUser()
        {
            try
            {
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync( x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                if (userInClaim.IsEntityUser == true)
                {
                    var organizationclaimcode = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == User.FindFirst(SD.OrganizationCode).Value);
                    var useradmin = await _unitofWork.AdministorUser.FirstOrDefaultAsync(x => x.UserCode == userInClaim.UserCode);
                    if (useradmin!=null)
                    {
                        var organizationibdb = await _unitofWork.DepartmentUser.GetAllAsync(x =>x.Office.OrganizationCode == organizationclaimcode.OrganizationCode || x.Office.Organization.ParentOrganizationCode == organizationclaimcode.OrganizationCode && x.Office.Organization.OrganizationType == "department",
                            includeProperties: "UserRole,Office.Organization,Designation,User.Ctv.District.State");

                        var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, organizationibdb)));

                        HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                        return Ok(new { data = okdata.EncryptedData });
                    }
                    else
                    {
                        var department = await _unitofWork.Office.FirstOrDefaultAsync(x => x.OfficeCode == User.FindFirst(SD.EntityCode).Value, includeProperties: "Organization");

                        var organizationibdb = await _unitofWork.DepartmentUser.GetAllAsync(
                        x => x.Office.OfficeCode == department.OfficeCode,
                        includeProperties: "UserRole,Office.Organization,Designation,User.Ctv.District.State");
                     var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, organizationibdb)));

                        HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                        return Ok(new { data = okdata.EncryptedData });
                    }
                }
                else
                {
                    var organizationibdb = await _unitofWork.DepartmentUser.GetAllAsync(
                        includeProperties: "UserRole,Office.Organization,Designation,User.Ctv.District.State");

                    var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, organizationibdb)));

                    HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                    return Ok(new { data = okdata.EncryptedData });
                }
            }
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));

                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);

                return StatusCode(500, new { data = data.EncryptedData });
            }
        }
        [HttpGet("getDepartmentUserForApproval")]
        public async Task<IActionResult> GetDepartmentUserForApproval()
        {
            try
            {
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(
                    x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));

                if (userInClaim.IsEntityUser == true)
                {
                    var organizationclaimcode = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == User.FindFirst(SD.OrganizationCode).Value);
                    var useradmin = await _unitofWork.AdministorUser.FirstOrDefaultAsync(x => x.UserCode == userInClaim.UserCode);

                    if (useradmin != null)
                    {
                        var organizationibdb = await _unitofWork.DepartmentUser.GetAllAsync(
                            x =>
                                x.Office.OrganizationCode == organizationclaimcode.OrganizationCode ||
                                x.Office.Organization.ParentOrganizationCode == organizationclaimcode.OrganizationCode
                                 && x.Office.Organization.OrganizationType == "department",
                            includeProperties: "UserRole,Office.Organization,Designation,User.Ctv.District.State");

                        var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, organizationibdb)));

                        HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                        return Ok(new { data = okdata.EncryptedData });
                    }
                    else
                    {
                        var organizationibdb = await _unitofWork.DepartmentUser.GetAllAsync(
                        x => x.Office.OrganizationCode == organizationclaimcode.OrganizationCode
                             && x.UserRole.RoleLevel > userRoleInClaim.RoleLevel ,
                        includeProperties: "UserRole,Office.Organization,Designation,User.Ctv.District.State");
                        var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                                   _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, organizationibdb)));

                        HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                        return Ok(new { data = okdata.EncryptedData });
                    }
                }
                else
                {
                    var organizationibdb = await _unitofWork.DepartmentUser.GetAllAsync(
                        includeProperties: "UserRole,Office.Organization,Designation,User.Ctv.District.State");

                    var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, organizationibdb)));

                    HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                    return Ok(new { data = okdata.EncryptedData });
                }
            }
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));

                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);

                return StatusCode(500, new { data = data.EncryptedData });
            }
        }

        [HttpPost("registerDepartmentUser")]
        public async Task<IActionResult> RegisterDepartmentUser([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    RegisterDepartmentUserVM user = JsonSerializer.Deserialize<RegisterDepartmentUserVM>(decryptedData);
                    if (TryValidateModel(user))
                    {
                        var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                        var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));



                        var requiredRole = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == user.RoleCode);

                        if (requiredRole == null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return NotFound(new { data = data.EncryptedData });
                        }

                        else if (userRoleInClaim.RoleLevel != RoleLevels.SUPREME && requiredRole.RoleLevel >= userRoleInClaim.RoleLevel)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }
                        var IsuniqueUser = await _unitofWork.User.IsUniqueUser(user.MobileNumber);

                        if (!IsuniqueUser)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }

                        var getuserOrganization = await _unitofWork.Office.FirstOrDefaultAsync(x => x.OfficeCode == user.OfficeCode);
                        if(getuserOrganization != null)
                        {
                            User user1 = new User()
                            {
                                UserCode = _unitofWork.User.GenrateUniqueCode(),
                                Name = user.Name,
                                Password = user.Password,
                                MobileNumber = user.MobileNumber,
                                EMail = user.EMail,
                                Address = user.Address,
                                CtvCode = user.CtvCode,
                                Token = user.Token,
                                IsActive = true,
                                CreatedBy = userInClaim.Name + "/" + userInClaim.UserCode,
                                CreatedOn = DateTime.Now.ToLocalTime(),
                                IsEntityUser = true,

                            };

                            await _unitofWork.User.RegisterUser(user1);
                            RoleAccess userrole = new RoleAccess
                            {
                                AccessId = _unitofWork.RoleAccess.GenrateUniqueCode(),
                                UserCode = user1.UserCode,
                                RoleCode = user.RoleCode,
                                AccessToRole = true
                            };

                            await _unitofWork.RoleAccess.AddAsync(userrole);

                            DepartmentUser departmentUser = new DepartmentUser()
                            {
                                DepartmentUserCode = _unitofWork.DepartmentUser.GenrateUniqueCode(),
                                OfficeCode = user.OfficeCode,
                                UserCode = user1.UserCode,
                                RoleCode = user.RoleCode,
                                DesignationCode = user.DesignationCode,
                                TotalStorageSize = user.TotalStorageSize,
                            };
                            await _unitofWork.DepartmentUser.AddAsync(departmentUser);

                            await _unitofWork.Office.UpdateAsync(user.OfficeCode, async entity =>
                            {
                                entity.AllocateStorageSize += user.TotalStorageSize;
                                await Task.CompletedTask;
                            });
                        }
                        var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Created, user)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                        return Ok(new { data = okdata.EncryptedData });
                    }
                    else
                        {
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                        (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, "Office Not Found")));
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

                var inDbRoles = (await _unitofWork.RoleAccess.GetAllAsync(
                    ra => ra.UserCode == indb.UserCode,
                    includeProperties: "User,UserRole")).ToList();

                if (inDbRoles.Any(r => r.UserRole.RoleLevel >= roleInClaim.RoleLevel))
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NoAccess, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });
                }

                await _unitofWork.User.ActiveDeactiveUser(indb.UserCode);

                var updatedUser = await _unitofWork.User.FirstOrDefaultAsync(d => d.UserCode == indb.UserCode);
                var userRoles = (await _unitofWork.RoleAccess.GetAllAsync(
                    ra => ra.UserCode == updatedUser.UserCode,
                    includeProperties: "UserRole"))
                    .OrderByDescending(x => x.UserRole.RoleLevel)
                    .ToList();

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

                var successData = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Updated, null)));
                HttpContext.Response.Headers.Append("X-Data-Hash", successData.Hash);

                return Ok(new { data = successData.EncryptedData });
            }
            catch (Exception ex)
            {
                var errorData = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(false, StatusType.failure, ValidationMessages.Default, ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", errorData.Hash);
                return StatusCode(500, new { data = errorData.EncryptedData });
            }
        }
        [HttpPost("updatedepartmentuserroleaccess")]
        [Authorize(Policy = SD.IsAccess)]
        public async Task<IActionResult> UpsertUserAndRoles([FromBody] EncryptedDataVM details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, details.Data);
                if (decryptedData == null)
                {
                    var encryptedResponse = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", encryptedResponse.Hash);
                    return BadRequest(new { data = encryptedResponse.EncryptedData });
                }
                var userAndRoles = JsonSerializer.Deserialize<RoleAccess>(decryptedData);
                if (userAndRoles == null)
                {
                    var encryptedResponse = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", encryptedResponse.Hash);
                    return BadRequest(new { data = encryptedResponse.EncryptedData });
                }
                var claimRoleCode = User.FindFirst(ClaimTypes.Role)?.Value;
                var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == claimRoleCode);

                if (userRoleInClaim == null)
                {
                    var encryptedResponse = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NoAccess, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", encryptedResponse.Hash);
                    return BadRequest(new { data = encryptedResponse.EncryptedData });
                }


                var requiredRole = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == userAndRoles.RoleCode);

                if (requiredRole == null ||
                    requiredRole.RoleLevel != RoleLevels.SUPREME && requiredRole.RoleLevel >= userRoleInClaim.RoleLevel)
                {
                    var encryptedResponse = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NoAccess, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", encryptedResponse.Hash);
                    return BadRequest(new { data = encryptedResponse.EncryptedData });
                }

                var userAndRoleInDb = await _unitofWork.RoleAccess.FirstOrDefaultAsync(
                    x => x.RoleCode == userAndRoles.RoleCode && x.UserCode == userAndRoles.UserCode);

                if (userAndRoleInDb == null)
                {
                    RoleAccess addUserAndRole = new RoleAccess
                    {
                        AccessId = _unitofWork.RoleAccess.GenrateUniqueCode(),
                        UserCode = userAndRoles.UserCode,
                        RoleCode = userAndRoles.RoleCode,
                        AccessToRole = true
                    };
                    await _unitofWork.RoleAccess.AddAsync(addUserAndRole);
                }
                else if (userAndRoleInDb.AccessToRole != userAndRoles.AccessToRole)
                {
                    await _unitofWork.RoleAccess.UpdateAsync(userAndRoleInDb.AccessId, async entity =>
                    {
                        entity.AccessToRole = userAndRoles.AccessToRole;
                        await Task.CompletedTask;
                    });
                }

                var successResponse = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Updated, null)));
                HttpContext.Response.Headers.Append("X-Data-Hash", successResponse.Hash);

                return Ok(new { data = successResponse.EncryptedData });
            }
            catch (Exception ex)
            {
                var errorResponse = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(false, StatusType.failure, ValidationMessages.Default, ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", errorResponse.Hash);
                return StatusCode(500, new { data = errorResponse.EncryptedData });
            }
        }
        [HttpPost("updatedepartmentuser")]
        [Authorize(Policy = SD.IsAccess)]
        public async Task<IActionResult> UpdateUser([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData == null)
                {
                    var resp = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));

                    HttpContext.Response.Headers.Append("X-Data-Hash", resp.Hash);
                    return BadRequest(new { data = resp.EncryptedData });
                }
                RegisterDepartmentUserVM user = JsonSerializer.Deserialize<RegisterDepartmentUserVM>(decryptedData);

                if (!TryValidateModel(user))
                {
                    var resp = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest,
                        ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList())));

                    HttpContext.Response.Headers.Append("X-Data-Hash", resp.Hash);
                    return BadRequest(new { data = resp.EncryptedData });
                }
                var userInClaim = await _unitofWork.User
                    .FirstOrDefaultAsync(d => d.UserCode == User.FindFirst(ClaimTypes.SerialNumber).Value);
                var roleInClaim = await _unitofWork.UserRole
                    .FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                if (user.UserCode != userInClaim.UserCode)
                {
                    var indb = await _unitofWork.User.FirstOrDefaultAsync(d => d.UserCode == user.UserCode);
                    if (indb == null)
                    {
                        var resp = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, ValidationMessages.NotFound)));

                        HttpContext.Response.Headers.Append("X-Data-Hash", resp.Hash);
                        return NotFound(new { data = resp.EncryptedData });
                    }

                    var inDbRoles = (await _unitofWork.RoleAccess.GetAllAsync(
                        ra => ra.UserCode == indb.UserCode,
                        includeProperties: "User,UserRole")).ToList();

                    if (roleInClaim.RoleLevel != RoleLevels.SUPREME &&
                        inDbRoles.Any(r => r.UserRole.RoleLevel >= roleInClaim.RoleLevel))
                    {
                        var resp = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NoAccess, ValidationMessages.NoAccess)));

                        HttpContext.Response.Headers.Append("X-Data-Hash", resp.Hash);
                        return BadRequest(new { data = resp.EncryptedData });
                    }
                    var getdepartmentuser = await _unitofWork.DepartmentUser.FirstOrDefaultAsync(x=>x.OfficeCode == user.OfficeCode && x.UserCode==user.UserCode ||x.UserCode==user.UserCode);
                      await _unitofWork.User.UpdateAsync(user.UserCode, async entity =>
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

                    await _unitofWork.DepartmentUser.UpdateAsync(getdepartmentuser.DepartmentUserCode, async entity =>
                    {
                        entity.OfficeCode = user.OfficeCode;
                        entity.TotalStorageSize = user.TotalStorageSize;
                        await Task.CompletedTask;
                    });
                     if(getdepartmentuser.OfficeCode == user.OfficeCode)
                    {
                        await _unitofWork.Office.UpdateAsync(getdepartmentuser.OfficeCode, async entity =>
                        {
                            entity.AllocateStorageSize = (entity.AllocateStorageSize) - getdepartmentuser.TotalStorageSize + user.TotalStorageSize;
                            await Task.CompletedTask;
                        });
                    }
                    else
                    {
                        await _unitofWork.Office.UpdateAsync(getdepartmentuser.OfficeCode, async entity =>
                        {
                            entity.AllocateStorageSize -= getdepartmentuser.TotalStorageSize;
                            if (entity.AllocateStorageSize < 0) entity.AllocateStorageSize = 0;
                            await Task.CompletedTask;
                        });
                        await _unitofWork.Office.UpdateAsync(user.OfficeCode, async entity =>
                        {
                            entity.AllocateStorageSize += user.TotalStorageSize;
                            await Task.CompletedTask;
                        });
                    }
                }
                else
                {
                    await _unitofWork.User.UpdateAsync(user.UserCode, async entity =>
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
                var okresp = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Updated, ValidationMessages.Updated)));

                HttpContext.Response.Headers.Append("X-Data-Hash", okresp.Hash);
                return Ok(new { data = okresp.EncryptedData });
            }
            catch (Exception Ex)
            {
                var resp = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));

                HttpContext.Response.Headers.Append("X-Data-Hash", resp.Hash);
                return StatusCode(500, new { data = resp.EncryptedData });
            }
        }
    }
}
