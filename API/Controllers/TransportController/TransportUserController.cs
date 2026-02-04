using API.Data.IRepositories;
using API.Data.Repositories;
using API.Helpers;
using API.Helpers.Models;
using API.Model.ManagementModels.TransporterManagement;
using API.Model.ManagementModels.UserModels;
using API.Model.QueryParamViewModels;
using API.Model.UserModels;
using API.Model.ViewModels;
using API.Model.ViewModels.TransporterManagementViewModels;
using API.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Intrinsics.X86;
using System.Security.Claims;
using System.Text.Json;

namespace API.Controllers.TransportController
{
    [ApiController]
    [Route(SD.baseUrl + "transportuser")]
    [Authorize(Policy = SD.IsAccess)]
    public class TransportUserController : ControllerBase
    {
        private readonly IUnitofWork _unitofWork;

        private readonly IEncryptionHelper _encryptionHelper;
        public TransportUserController(IUnitofWork unitofWork, IEncryptionHelper encryptionHelper)
        {
            _unitofWork = unitofWork;
            _encryptionHelper = encryptionHelper;
        }

        [HttpGet("getAllTransportUser")]
        public async Task<IActionResult> GetAllTransportUser()
        {
            try
            {
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(
                    x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));

                if (userInClaim.IsEntityUser == false)
                {
                    var organizationclaimcode = await _unitofWork.Organization.FirstOrDefaultAsync(
                        x => x.OrganizationCode == User.FindFirst(SD.OrganizationCode).Value);

                    var userontranportdb = await _unitofWork.AdministorUser.FirstOrDefaultAsync(
                        x => x.UserCode == userInClaim.UserCode);

                    if (userontranportdb==null)
                    {
                        var organizationibdb = await _unitofWork.TransportUser.GetAllAsync(
                            x =>
                                x.OrganizationCode == organizationclaimcode.OrganizationCode ||
                                x.Organization.ParentOrganizationCode == organizationclaimcode.OrganizationCode,
                            includeProperties: "UserRole,Organization,User");

                        var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, organizationibdb)));

                        HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                        return Ok(new { data = okdata.EncryptedData });
                    }
                    else
                    {
                        var organizationibdb = await _unitofWork.TransportUser.GetAllAsync(
                        x => x.OrganizationCode == organizationclaimcode.OrganizationCode
                             && x.UserRole.RoleLevel <= userRoleInClaim.RoleLevel,
                        includeProperties: "UserRole,Organization,User");


                        var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, organizationibdb)));

                        HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                        return Ok(new { data = okdata.EncryptedData });
                    }
                }
                else
                {
                    var organizationibdb = await _unitofWork.TransportUser.GetAllAsync(
                        includeProperties: "UserRole,Organization,User");

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

        [HttpGet("getAllDriver")]
        public async Task<IActionResult> GetAllDriverUser()
        {
            try
            {
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(
                    x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                if (userInClaim.IsEntityUser == true)
                {
                    var organizationClaim = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == User.FindFirst(SD.OrganizationCode).Value);
                    var userOnTransport = await _unitofWork.AdministorUser.FirstOrDefaultAsync(x => x.UserCode == userInClaim.UserCode && x.OrganizationCode == organizationClaim.OrganizationCode, includeProperties: "UserRole,Organization,User");
                    var allDriverUsers = await _unitofWork.TransportUser.GetAllAsync(tu => tu.IsDriver == true, includeProperties: "UserRole,Organization,User.Ctv.District.State"
                    );

                    List<DriverDetails> driverList = new List<DriverDetails>();

                    if (userOnTransport != null)
                    {
                        var allowedUserCodes = allDriverUsers
                            .Where(tu => tu.OrganizationCode == organizationClaim.OrganizationCode
                                      || tu.Organization.ParentOrganizationCode == organizationClaim.OrganizationCode)
                            .Where(tu => tu.UserRole.RoleLevel <= userRoleInClaim.RoleLevel)
                            .Select(tu => tu.UserCode)
                            .ToList();

                        if (!allowedUserCodes.Any())
                        {
                            driverList = new List<DriverDetails>();
                        }
                        else
                        {
                            driverList = (await _unitofWork.DriverDetails.GetAllAsync(
                                d => allowedUserCodes.Contains(d.UserCode),
                                includeProperties: "User.Ctv.District.State"
                            )).ToList();
                        }
                    }
                    else
                    {
                        var allowedUserCodes = allDriverUsers
                            .Where(tu => tu.OrganizationCode == organizationClaim.OrganizationCode
                                      && tu.UserRole.RoleLevel <= userRoleInClaim.RoleLevel)
                            .Select(tu => tu.UserCode)
                            .ToList();

                        if (!allowedUserCodes.Any())
                        {
                            driverList = new List<DriverDetails>();
                        }
                        else
                        {
                            driverList = (await _unitofWork.DriverDetails.GetAllAsync(
                                d => allowedUserCodes.Contains(d.UserCode),
                                includeProperties: "User.Ctv.District.State"
                            )).ToList();
                        }
                    }

                    var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, driverList)));

                    HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                    return Ok(new { data = okdata.EncryptedData });
                }
                else
                {
                    var allDrivers = await _unitofWork.DriverDetails.GetAllAsync(includeProperties: "User.Ctv.District.State");

                    var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, allDrivers)));

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
        [HttpPost("registerTransportUser")]
        public async Task<IActionResult> RegisterTransportUser([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    RegisterTransportUserVM user = JsonSerializer.Deserialize<RegisterTransportUserVM>(decryptedData);
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
                            var getuserOrganization = await _unitofWork.Organization
                                .FirstOrDefaultAsync(x => x.OrganizationCode == user.OrganizationCode);

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
                                IsEntityUser=true,
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

                            TransporterUser transporterUser = new TransporterUser()
                            {
                                TransportUserCode = _unitofWork.TransportUser.GenrateUniqueCode(),
                                OrganizationCode = user.OrganizationCode,
                                UserCode = user1.UserCode,
                                IsDriver = true,
                                RoleCode = user.RoleCode,
                            };

                            await _unitofWork.TransportUser.AddAsync(transporterUser);

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
        [HttpPost("updateTransportuserroleaccess")]
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

                var userAndRoles = JsonSerializer.Deserialize<RoleAccess[]>(decryptedData);
                if (userAndRoles == null || userAndRoles.Length == 0)
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

                foreach (var userAndRole in userAndRoles)
                {
                    var requiredRole = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == userAndRole.RoleCode);

                    if (requiredRole == null ||
                        requiredRole.RoleLevel != RoleLevels.SUPREME && requiredRole.RoleLevel >= userRoleInClaim.RoleLevel)
                    {
                        var encryptedResponse = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NoAccess, null)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", encryptedResponse.Hash);
                        return BadRequest(new { data = encryptedResponse.EncryptedData });
                    }

                    var userAndRoleInDb = await _unitofWork.RoleAccess.FirstOrDefaultAsync(
                        x => x.RoleCode == userAndRole.RoleCode && x.UserCode == userAndRole.UserCode);

                    if (userAndRoleInDb == null)
                    {
                        RoleAccess addUserAndRole = new RoleAccess
                        {
                            AccessId = _unitofWork.RoleAccess.GenrateUniqueCode(),
                            UserCode = userAndRole.UserCode,
                            RoleCode = userAndRole.RoleCode,
                            AccessToRole = true
                        };
                        await _unitofWork.RoleAccess.AddAsync(addUserAndRole);
                    }
                    else if (userAndRoleInDb.AccessToRole != userAndRole.AccessToRole)
                    {
                        await _unitofWork.RoleAccess.UpdateAsync(userAndRoleInDb.AccessId, async entity =>
                        {
                            entity.AccessToRole = userAndRole.AccessToRole;
                            await Task.CompletedTask;
                        });
                    }
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
        [HttpPost("registerDriver")]
        public async Task<IActionResult> RegisterDriverForTransport([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    RegisterDriverVM user = JsonSerializer.Deserialize<RegisterDriverVM>(decryptedData);
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
                            return BadRequest(new { data = data.EncryptedData });
                        }
                        else
                        {
                            var getuserOrganization = await _unitofWork.Organization
                                .FirstOrDefaultAsync(x => x.OrganizationCode == user.OrganizationCode);

                            User user1 = new User()
                            {
                                UserCode = _unitofWork.User.GenrateUniqueCode(),
                                Name = user.Name,
                                Password = user.Password,
                                MobileNumber = user.MobileNumber,
                                EMail = user.EMail,
                                Address = user.Address,
                                CtvCode = user.CtvCode,
                                IsEntityUser =true,
                                Token = user.Token,
                                IsActive = true,
                                CreatedBy = userInClaim.Name + "/" + userInClaim.UserCode,
                                CreatedOn = DateTime.Now.ToLocalTime(),

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

                            TransporterUser transporterUser = new TransporterUser()
                            {
                                TransportUserCode = _unitofWork.TransportUser.GenrateUniqueCode(),
                                OrganizationCode = user.OrganizationCode,
                                UserCode = user1.UserCode,
                                RoleCode = user.RoleCode,
                                IsDriver=true,
                            };
                            await _unitofWork.TransportUser.AddAsync(transporterUser);
                            DriverDetails driverDetails = new DriverDetails()
                            {
                                DriverDetailCode = _unitofWork.DriverDetails.GenrateUniqueCode(), 
                                DriverName = user1.Name,             
                                LicenseNumber = user.LicenseNumber,
                                LicenseType = user.LicenseType,    
                                LicenseExpiryDate = user.LicenseExpiryDate.ToLocalTime(),
                                DateOfBirth = user.DateOfBirth.ToLocalTime(),
                                EmergencyContact = user.EmergencyContact,
                                BloodGroup = user.BloodGroup,
                                UserCode = user1.UserCode,
                            };
                            await _unitofWork.DriverDetails.AddAsync(driverDetails);

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
        [HttpPost("updatetransportuser")]
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
                User user = JsonSerializer.Deserialize<User>(decryptedData);

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
        [HttpPost("updatedriverdetails")]
        public async Task<IActionResult> AdddrvierDetails([FromBody] EncryptedDataVM Details)
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
                RegisterDriverVM user = JsonSerializer.Deserialize<RegisterDriverVM>(decryptedData);


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
                    var userdetails = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == user.UserCode);
                    var driverdetails = await _unitofWork.DriverDetails.FirstOrDefaultAsync(x => x.UserCode == userdetails.UserCode);


                    await _unitofWork.DriverDetails.UpdateAsync(driverdetails.DriverDetailCode, async entity =>
                    {
                        entity.DriverName = user.Name;
                        entity.LicenseNumber = user.LicenseNumber;
                        entity.LicenseType = user.LicenseType;
                        entity.LicenseExpiryDate = user.LicenseExpiryDate.ToLocalTime();
                        entity.DateOfBirth = user.DateOfBirth.ToLocalTime();
                        entity.EmergencyContact = user.EmergencyContact;
                        entity.BloodGroup = user.BloodGroup;
                        await Task.CompletedTask;
                    });
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

                }
                else
                {
                    var userdetails = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == user.UserCode);
                    var driverdetails = await _unitofWork.DriverDetails.FirstOrDefaultAsync(x => x.UserCode == userdetails.UserCode);


                    await _unitofWork.DriverDetails.UpdateAsync(driverdetails.DriverDetailCode, async entity =>
                    {
                        entity.DriverName = user.Name;
                        entity.LicenseNumber = user.LicenseNumber;
                        entity.LicenseType = user.LicenseType;
                        entity.LicenseExpiryDate = user.LicenseExpiryDate.ToLocalTime();
                        entity.DateOfBirth = DateTime.SpecifyKind(user.DateOfBirth, DateTimeKind.Utc);
                        entity.EmergencyContact = user.EmergencyContact;
                        entity.BloodGroup = user.BloodGroup;
                        await Task.CompletedTask;
                    });
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
