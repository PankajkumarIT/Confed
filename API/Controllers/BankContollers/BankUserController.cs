using API.Data.IRepositories;
using API.Helpers;
using API.Helpers.Models;
using API.Model.ManagementModels.UserModels;
using API.Model.UserModels;
using API.Model.ViewModels;
using API.Model.ViewModels.TransporterManagementViewModels;
using API.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace API.Controllers.BankContollers
{
    [ApiController]
    [Route(SD.baseUrl + "bankuser")]
    [Authorize(Policy = SD.IsAccess)]
    public class BankUserController : ControllerBase
    {
        private readonly IUnitofWork _unitofWork;

        private readonly IEncryptionHelper _encryptionHelper;
        public BankUserController(IUnitofWork unitofWork, IEncryptionHelper encryptionHelper)
        {
            _unitofWork = unitofWork;
            _encryptionHelper = encryptionHelper;
        }

        [HttpGet("getAllBankUser")]
        public async Task<IActionResult> GetAllBankUser()
        {
            try
            {
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(
                    x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));

                if (userInClaim.IsEntityUser == true)
                {
                    var organizationclaimcode = await _unitofWork.Organization.FirstOrDefaultAsync(
                        x => x.OrganizationCode == User.FindFirst(SD.OrganizationCode).Value);

                    var useradmin = await _unitofWork.AdministorUser.FirstOrDefaultAsync(
                        x => x.UserCode == userInClaim.UserCode);

                    if (useradmin != null)
                    {
                        var organizationibdb = await _unitofWork.BankUser.GetAllAsync(
                            x =>
                                x.BankBranch.OrganizationCode == organizationclaimcode.OrganizationCode ||
                                x.BankBranch.Organization.ParentOrganizationCode == organizationclaimcode.OrganizationCode
                                 && x.BankBranch.Organization.OrganizationType == "bank",
                            includeProperties: "UserRole,BankBranch.Organization,Designation,User.Ctv.District.State");

                        var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, organizationibdb)));

                        HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                        return Ok(new { data = okdata.EncryptedData });
                    }
                    else
                    {
                        var department = await _unitofWork.BankBranch.FirstOrDefaultAsync(x => x.BankBranchCode == User.FindFirst(SD.EntityCode).Value, includeProperties: "Organization");

                        var organizationibdb = await _unitofWork.BankUser.GetAllAsync(
                        x => x.BankBranch.BankBranchCode == department.BankBranchCode,
                        includeProperties: "UserRole,BankBranch.Organization,Designation,User.Ctv.District.State");


                        var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, organizationibdb)));

                        HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                        return Ok(new { data = okdata.EncryptedData });
                    }
                }
                else
                {
                    var organizationibdb = await _unitofWork.BankUser.GetAllAsync(
                        includeProperties: "UserRole,BankBranch.Organization,BankBranch.BankBranch,User.Ctv.District.State");

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
        [HttpPost("registerBankUser")]
        public async Task<IActionResult> RegisterDepartmentUser([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    RegisterBankUserVM user = JsonSerializer.Deserialize<RegisterBankUserVM>(decryptedData);
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
                            return Ok(new { data = data.EncryptedData });
                        }

                        else if (userRoleInClaim.RoleLevel != RoleLevels.SUPREME && requiredRole.RoleLevel >= userRoleInClaim.RoleLevel)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return Ok(new { data = data.EncryptedData });
                        }


                        var IsuniqueUser = await _unitofWork.User.IsUniqueUser(user.MobileNumber);
                        if (!IsuniqueUser)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return Ok(new { data = data.EncryptedData });
                        }
                        else
                        {
                            var getuserOrganization = await _unitofWork.BankBranch
                                .FirstOrDefaultAsync(x => x.BankBranchCode == user.BankBranchCode);

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

                            BankUser bankUser = new BankUser()
                            {
                                BankUserCode = _unitofWork.BankUser.GenrateUniqueCode(),
                                BankBranchCode = user.BankBranchCode,
                                UserCode = user1.UserCode,
                                TotalStorageSize = user.TotalStorageSize,
                                RoleCode = user.RoleCode,                                
                                DesignationCode = user.DesignationCode
                            };
                            await _unitofWork.BankUser.AddAsync(bankUser);

                            await _unitofWork.BankBranch.UpdateAsync(user.BankBranchCode, async entity =>
                            {
                                entity.AllocateStorageSize += user.TotalStorageSize;
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
        [HttpPost("updatebankuserroleaccess")]
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
                if (userAndRoles == null )
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
        [HttpPost("updatebankuser")]
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
                RegisterBankUserVM user = JsonSerializer.Deserialize<RegisterBankUserVM>(decryptedData);

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
                    var getbankuser = await _unitofWork.BankUser.FirstOrDefaultAsync(x => x.BankBranchCode == user.BankBranchCode && x.UserCode == user.UserCode ||x.UserCode==user.UserCode);

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
                    await _unitofWork.BankUser.UpdateAsync(getbankuser.BankUserCode, async entity =>
                    {
                        entity.BankBranchCode = user.BankBranchCode;
                        entity.TotalStorageSize = user.TotalStorageSize;
                        await Task.CompletedTask;
                    });
                    if (getbankuser.BankBranchCode == user.BankBranchCode)
                    {
                        await _unitofWork.BankBranch.UpdateAsync(getbankuser.BankBranchCode, async entity =>
                        {
                            entity.AllocateStorageSize = (entity.AllocateStorageSize) - getbankuser.TotalStorageSize + user.TotalStorageSize;
                            await Task.CompletedTask;
                        });
                    }
                    else
                    {
                        await _unitofWork.BankBranch.UpdateAsync(getbankuser.BankBranchCode, async entity =>
                        {
                            entity.AllocateStorageSize -= getbankuser.TotalStorageSize;
                            if (entity.AllocateStorageSize < 0) entity.AllocateStorageSize = 0;
                            await Task.CompletedTask;
                        });
                        await _unitofWork.BankBranch.UpdateAsync(user.BankBranchCode, async entity =>
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
