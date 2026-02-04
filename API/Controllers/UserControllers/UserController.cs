using API.Data;
using API.Data.IRepositories;
using API.Helpers;
using API.Helpers.Models;
using API.Model.ManagementModels.TransporterManagement;
using API.Model.QueryParamViewModels;
using API.Model.UserModels;
using API.Model.ViewModels.TransporterManagementViewModels;
using API.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Xml.Linq;

namespace API.Controllers.UserControllers
{
    [Route(SD.baseUrl + "user")]
    [ApiController]

    public class UserController : Controller
    {
        private readonly IUnitofWork _iunitofwork;
        private readonly IEncryptionHelper _encryptionHelper;

        public UserController(IUnitofWork unitofwork, ApplicationDbContext context,IEncryptionHelper encryptionHelper)
        {
            _iunitofwork = unitofwork;
            _encryptionHelper = encryptionHelper;
        }

        [HttpPost("users")]
        [Authorize(Policy = SD.IsAccess)]
        public async Task<IActionResult> GetUsers([FromBody] EncryptedDataVM details)
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
                string roleCode = requestData?.Value;

                var userRoleInClaim = await _iunitofwork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                var usersToRemove = new List<RoleAccess>();
                List<RoleAccess> users = new List<RoleAccess>();

                if (string.IsNullOrEmpty(roleCode))
                {
                    var requiredRole = await _iunitofwork.UserRole.FirstOrDefaultAsync(d => d.RoleLevel == RoleLevels.PRIMARY);
                    users = (await _iunitofwork.RoleAccess.GetAllAsync(x => x.RoleCode == requiredRole.RoleCode, includeProperties: "User.Ctv.District.State,UserRole")).ToList();
                }
                else
                {
                    var requiredRole = await _iunitofwork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == roleCode);

                    if (userRoleInClaim.RoleLevel != RoleLevels.SUPREME && requiredRole.RoleLevel >= userRoleInClaim.RoleLevel)
                    {
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(false, StatusType.failure, ValidationMessages.NoAccess, null)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                        return BadRequest(new { data = data.EncryptedData });
                    }

                    users = (await _iunitofwork.RoleAccess.GetAllAsync(x => x.RoleCode == requiredRole.RoleCode, includeProperties: "User.Ctv.District.State,UserRole")).ToList();
                }

                foreach (var user in users)
                {
                    var rolesInDb = await _iunitofwork.RoleAccess.GetAllAsync(u => u.UserCode == user.UserCode, includeProperties: "User.Ctv.District.State,UserRole");
                    if (rolesInDb.Any(r => userRoleInClaim.RoleLevel != RoleLevels.SUPREME && r.UserRole.RoleLevel >= userRoleInClaim.RoleLevel))
                        usersToRemove.Add(user);
                }

                foreach (var user in usersToRemove)
                {
                    users.Remove(user);

                }

                var successData = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, users)));
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

        [HttpPost("registerusers")]
        [Authorize(Policy = SD.IsAccess)]
        public async Task<IActionResult> RegisterUsers([FromBody] EncryptedDataVM Details)
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
                RegisterUserVM user = JsonSerializer.Deserialize<RegisterUserVM>(decryptedData);

                if (!TryValidateModel(user))
                {
                    var resp = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest,
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList())));

                    HttpContext.Response.Headers.Append("X-Data-Hash", resp.Hash);
                    return BadRequest(new { data = resp.EncryptedData });
                }

                var userRoleInClaim = await _iunitofwork.UserRole
                    .FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));

                var userInClaim = await _iunitofwork.User
                    .FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                var requiredRole = await _iunitofwork.UserRole
                    .FirstOrDefaultAsync(d => d.RoleCode == user.RoleCode);

                if (requiredRole == null)
                {
                    var resp = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, ValidationMessages.NotFound)));

                    HttpContext.Response.Headers.Append("X-Data-Hash", resp.Hash);
                    return NotFound(new { data = resp.EncryptedData });
                }

                if (userRoleInClaim.RoleLevel != RoleLevels.SUPREME &&
                    requiredRole.RoleLevel >= userRoleInClaim.RoleLevel)
                {
                    var resp = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NoAccess, ValidationMessages.NoAccess)));

                    HttpContext.Response.Headers.Append("X-Data-Hash", resp.Hash);
                    return BadRequest(new { data = resp.EncryptedData });
                }

                var isuniqueuser = await _iunitofwork.User.IsUniqueUser(user.MobileNumber);
                if (!isuniqueuser)
                {
                    var resp = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, ValidationMessages.Exists)));

                    HttpContext.Response.Headers.Append("X-Data-Hash", resp.Hash);
                    return BadRequest(new { data = resp.EncryptedData });
                }

                User adduser = new User()
                {
                    UserCode = _iunitofwork.User.GenrateUniqueCode(),
                    Name = user.Name,
                    Password = user.Password,
                    MobileNumber = user.MobileNumber,
                    EMail = user.EMail,
                    Address = user.Address,
                    CtvCode = user.CtvCode,
                    IsActive = true,
                    IsEntityUser = false,
                    CreatedBy = userInClaim.Name + "/" + userInClaim.UserCode,
                    CreatedOn = DateTime.Now.ToLocalTime()
                };

                RoleAccess userrole = new RoleAccess
                {
                    AccessId = _iunitofwork.RoleAccess.GenrateUniqueCode(),
                    UserCode = adduser.UserCode,
                    RoleCode = requiredRole.RoleCode,
                    AccessToRole = true
                };

                await _iunitofwork.User.RegisterUser(adduser);
                await _iunitofwork.RoleAccess.AddAsync(userrole);

                var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Created, ValidationMessages.Created)));

                HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                return Ok(new { data = okdata.EncryptedData });
            }
            catch (Exception Ex)
            {
                var resp = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));

                HttpContext.Response.Headers.Append("X-Data-Hash", resp.Hash);
                return StatusCode(500, new { data = resp.EncryptedData });
            }
        }
        [HttpPost("userlogins")]
        [Authorize(Policy = SD.IsAccess)]
        public async Task<IActionResult> GetUserLogins([FromBody] EncryptedDataVM details)
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
                string userCode = requestData?.Value;

                if (string.IsNullOrEmpty(userCode))
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });
                }
                var logins = await _iunitofwork.UserLogin.GetAllAsync(x => x.UserCode == userCode);

                var successData = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, logins)));
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
        [HttpPost("user")]
        [Authorize(Policy = SD.IsAccess)]
        public async Task<IActionResult> GetUser(string UserCode, string RoleCode)
        {
            try
            {
                var userRoleInClaim = await _iunitofwork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirst(ClaimTypes.Role).Value);
                if (RoleCode == null)
                {
                    return BadRequest(new { message = ValidationMessages.BadRequest });
                }
                var decryptedroleCode = _iunitofwork.User.DecrypteBase64(RoleCode);
                var requiredRole = await _iunitofwork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == decryptedroleCode);

                if (requiredRole == null)
                    return NotFound(new { message = ValidationMessages.NotFound });

                else if (userRoleInClaim.RoleLevel != RoleLevels.SUPREME && requiredRole.RoleLevel >= userRoleInClaim.RoleLevel)
                    return BadRequest(new { message = ValidationMessages.NoAccess });
                else
                {
                    if (UserCode == null)
                    {
                        return BadRequest(new { message = ValidationMessages.BadRequest });
                    }
                    var decryptedUserCode = _iunitofwork.User.DecrypteBase64(UserCode);

                    var user = await _iunitofwork.RoleAccess.FirstOrDefaultAsync(filter: d => d.UserCode == decryptedUserCode && d.RoleCode == requiredRole.RoleCode, includeProperties: "User.Ctv.District.State,UserRole");

                    if (user == null)
                        return NotFound(new { message = ValidationMessages.NotFound });
                    else
                    {
                        var rolesindb = await _iunitofwork.RoleAccess.GetAllAsync(u => u.UserCode == user.UserCode, includeProperties: "User,UserRole");

                        if (rolesindb.Any(r => userRoleInClaim.RoleLevel != RoleLevels.SUPREME && r.UserRole.RoleLevel >= userRoleInClaim.RoleLevel))

                            return BadRequest(new { message = ValidationMessages.NoAccess });

                        return Ok(user);
                    }
                }
            }
            catch (Exception Ex)
            {
                return StatusCode(500, new { message = ValidationMessages.Default, exception = Ex });
            }

        }
        [HttpGet("userbyclaim")]

        public async Task<IActionResult> GetUserByClaim()

        {

            try

            {

                var userInClaim = await _iunitofwork.User.FirstOrDefaultAsync(d => d.UserCode == User.FindFirst(ClaimTypes.SerialNumber).Value);

                var RoleInClaim = await _iunitofwork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirst(ClaimTypes.Role).Value);

                var rolesindb = await _iunitofwork.RoleAccess.GetAllAsync(u => u.UserCode == userInClaim.UserCode && u.AccessToRole == true, includeProperties: "User.Ctv.District.State,UserRole");

                List<UserRole> roles = new List<UserRole>();

                userInClaim.Password = null;

                foreach (var role in rolesindb)
                {

                    var userRole = role.UserRole;

                    roles.Add(userRole);

                }

                UserInClaimVM user = new UserInClaimVM()

                {

                    User = userInClaim,

                    UserRoles = roles,

                    CurrentUserRole = RoleInClaim,

                };

                if (userInClaim.IsEntityUser == true)

                {

                    var UserTypeInClaim = User.FindFirst(SD.UserType).Value;

                    if (UserTypeInClaim == SD.DepartmentUser)

                    {

                        var indb = await _iunitofwork.DepartmentUser.FirstOrDefaultAsync(d => d.DepartmentUserCode == User.FindFirst(SD.UserTypeCode).Value, includeProperties: "Office");

                        user.DepartmentUser = indb;

                        var organisationindb = await _iunitofwork.Organization.FirstOrDefaultAsync(d => d.OrganizationCode == indb.Office.OrganizationCode);

                        user.Organization = organisationindb;

                    }

                    else if (UserTypeInClaim == SD.BankUser)

                    {

                        var indb = await _iunitofwork.BankUser.FirstOrDefaultAsync(d => d.BankUserCode == User.FindFirst(SD.UserTypeCode).Value, includeProperties: "BankBranch");

                        user.BankUser = indb;

                        var organisationindb = await _iunitofwork.Organization.FirstOrDefaultAsync(d => d.OrganizationCode == indb.BankBranch.OrganizationCode);

                        user.Organization = organisationindb;

                    }

                    else if (UserTypeInClaim == SD.TransportUser)

                    {
                            
                        var indb = await _iunitofwork.TransportUser.FirstOrDefaultAsync(d => d.TransportUserCode == User.FindFirst(SD.UserTypeCode).Value);

                        user.TransporterUser = indb;

                        var organisationindb = await _iunitofwork.Organization.FirstOrDefaultAsync(d => d.OrganizationCode == indb.OrganizationCode);

                        user.Organization = organisationindb;

                    }

                    else if (UserTypeInClaim == SD.AdminstratorUser)

                    {

                        var indb = await _iunitofwork.AdministorUser.FirstOrDefaultAsync(d => d.AdministorCode == User.FindFirst(SD.UserTypeCode).Value);

                        user.Administor = indb;

                        var organisationindb = await _iunitofwork.Organization.FirstOrDefaultAsync(d => d.OrganizationCode == indb.OrganizationCode);

                        user.Organization = organisationindb;

                    }

                }

                var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic> (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, user)));

                HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);

                return Ok(new { data = okdata.EncryptedData });


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

        [HttpPost("updatepassword")]
        public async Task<IActionResult> UpdateUserPassword([FromBody] EncryptedDataVM details)
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
                var userVM = JsonSerializer.Deserialize<ChangePasswordVM>(decryptedData);
                if (!TryValidateModel(userVM))
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });
                }
                if (string.IsNullOrEmpty(userVM.UserCode))
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });
                }
                var inClaimUser = User.FindFirstValue(ClaimTypes.SerialNumber);

                if (userVM.UserCode != inClaimUser)
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NoAccess, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });
                }
                var userInDb = await _iunitofwork.User.FirstOrDefaultAsync(
                    u => u.UserCode == userVM.UserCode && u.Password == userVM.OldPassword);

                if (userInDb == null)
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotMatched, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return NotFound(new { data = data.EncryptedData });
                }
                await _iunitofwork.User.UpdateAsync(userInDb.UserCode, async entity =>
                {
                    entity.Password = userVM.NewPassword;
                    entity.UpdatedOn = DateTime.Now.ToLocalTime();
                    await Task.CompletedTask;
                });

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

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] EncryptedDataVM details)
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

                var userVM = JsonSerializer.Deserialize<AuthenticateVM>(decryptedData);
                if (!TryValidateModel(userVM))
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });
                }

                HttpContext.Request.Headers.TryGetValue("device-Id", out var deviceId);
                var userallInDb = await _iunitofwork.User.GetAllAsync();


                var userInDb = await _iunitofwork.User.FirstOrDefaultAsync(u => u.MobileNumber == userVM.UserName);
                if (userInDb == null)
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return NotFound(new { data = data.EncryptedData });
                }

                if (userInDb.Password != userVM.Password)
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.WrongPassword, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });
                }

                if (!userInDb.IsActive)
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.InActiveUser, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });
                }

                var inDbRoles = (await _iunitofwork.RoleAccess.GetAllAsync(
                    ra => ra.UserCode == userInDb.UserCode && ra.AccessToRole,
                    includeProperties: "UserRole"))
                    .OrderByDescending(x => x.UserRole.RoleLevel)
                    .ToList();

                var userRole = inDbRoles.FirstOrDefault(x => x.AccessToRole);
                if (userRole == null)
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NoAccess, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });
                }


                var tokenindb = await _iunitofwork.User.Authenticate(userVM.UserName, userRole.UserRole.RoleCode);

                await _iunitofwork.User.UpdateAsync(userInDb.UserCode, async entity =>
                {
                    entity.LastLogin = DateTime.Now.ToLocalTime();
                    entity.Token = tokenindb;
                    await Task.CompletedTask;
                });
                UserLogin login = new UserLogin()
                {
                    LoginCode = _iunitofwork.UserLogin.GenrateUniqueCode(),
                    UserCode = userInDb.UserCode,
                    LoginTime = DateTime.Now.ToLocalTime()
                };
                await _iunitofwork.UserLogin.AddAsync(login);

                Response.Cookies.Append(
                      "AuthToken",
                      tokenindb,
                      new CookieOptions
                      {
                          HttpOnly = true,
                          Secure = true,
                          SameSite = SameSiteMode.None,
                          IsEssential = true,
                          Path = "/",
                          Expires = DateTime.Now.ToLocalTime().AddHours(24)
                      }
                  );

                var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, null)));
                HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                return Ok(new { data = okdata.EncryptedData });
            }
            catch (Exception ex)
            {
                var errorData = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(false, StatusType.failure, ValidationMessages.Default, ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", errorData.Hash);

                return StatusCode(500, new { data = errorData.EncryptedData });
            }
        }
        [HttpGet("logout")]

        public async Task<IActionResult> Logout()

        {

            var userInClaim = await _iunitofwork.User.FirstOrDefaultAsync(d => d.UserCode == User.FindFirst(ClaimTypes.SerialNumber).Value);

            await _iunitofwork.User.UpdateAsync(userInClaim.UserCode, async entity =>

            {

                entity.Token = null;

                await Task.CompletedTask;

            });

            Response.Cookies.Append(

                "AuthToken",

                "",

                new CookieOptions

                {

                    HttpOnly = true,

                    Secure = true,

                    SameSite = SameSiteMode.None,

                    Path = "/",

                    Expires = DateTime.Now.ToLocalTime().AddDays(-1)

                }

            );

            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>

                (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, "Logged out successfully")));

            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);

            return Ok(new { data = data.EncryptedData });

        }


        [HttpPost("changeRole")]
        [Authorize(Policy = SD.IsAccess)]
        public async Task<IActionResult> ChangeRole([FromBody] EncryptedDataVM Details)
        {
            try
            {

                var userCodeInClaim = User.FindFirstValue(ClaimTypes.SerialNumber);

                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    RoleCodeVM CodeVM = JsonSerializer.Deserialize<RoleCodeVM>(decryptedData);


                    if (TryValidateModel(CodeVM))
                    {

                        var requiredRole = await _iunitofwork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == CodeVM.RoleCode);
                        var userindb = await _iunitofwork.User.FirstOrDefaultAsync(u => u.UserCode == userCodeInClaim);
                        if (userindb == null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                            (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return NotFound(new { data = data.EncryptedData });
                        }

                        else
                        {
                            var userRole = await _iunitofwork.RoleAccess.FirstOrDefaultAsync(ra => ra.UserCode == userindb.UserCode && ra.AccessToRole == true && ra.RoleCode == requiredRole.RoleCode, includeProperties: "User.Ctv.District.State,UserRole");
                            if (userRole == null)
                            {
                                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NoAccess, null)));
                                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                                return Unauthorized(new { data = data.EncryptedData });
                            }
                            else
                            {
                                var tokenindb = await _iunitofwork.User.Authenticate(userindb.MobileNumber, userRole.UserRole.RoleCode);

                                await _iunitofwork.User.UpdateAsync(userindb.UserCode, async entity =>
                                {
                                    entity.LastLogin = DateTime.Now.ToLocalTime();
                                    await Task.CompletedTask;
                                });

                                UserLogin login = new UserLogin()
                                {
                                    LoginCode = _iunitofwork.UserLogin.GenrateUniqueCode(),
                                    UserCode = userindb.UserCode,
                                    LoginTime = DateTime.Now.ToLocalTime()
                                };
                                await _iunitofwork.UserLogin.AddAsync(login);
                                Response.Cookies.Append(
                                     "AuthToken",
                                     tokenindb,
                                     new CookieOptions
                                     {
                                         HttpOnly = true,
                                         Secure = true,
                                         SameSite = SameSiteMode.Strict,
                                         IsEssential = true,
                                         Path = "/",
                                         Expires = DateTime.Now.ToLocalTime().AddHours(24)
                                     }
                                 );

                                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic> (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Updated, null)));
                                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                                return Ok(new { data = data.EncryptedData });
                            }
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
                return StatusCode(500, new { data = data.EncryptedData });
            }

        }
        [HttpDelete("delete")]
        [Authorize(Policy = SD.IsAccess)]
        public async Task<IActionResult> DeleteUser([FromBody] EncryptedDataVM details)
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
                string userCode = requestData?.Value;

                if (string.IsNullOrEmpty(userCode))
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });
                }

                var roleInClaim = await _iunitofwork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                var indb = await _iunitofwork.User.FirstOrDefaultAsync(d => d.UserCode == userCode);

                if (indb == null)
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return NotFound(new { data = data.EncryptedData });
                }

                var inDbRoles = (await _iunitofwork.RoleAccess.GetAllAsync(
                    ra => ra.UserCode == indb.UserCode,
                    includeProperties: "User,UserRole")).ToList();

                if (inDbRoles.Any(r => r.UserRole.RoleLevel >= roleInClaim.RoleLevel))
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NoAccess, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });
                }

                await _iunitofwork.User.RemoveAsync(indb);

                var successData = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Deleted, null)));
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
                var roleInClaim = await _iunitofwork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                var indb = await _iunitofwork.User.FirstOrDefaultAsync(d => d.UserCode == requestData.Value);

                if (indb == null)
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return NotFound(new { data = data.EncryptedData });
                }

                var inDbRoles = (await _iunitofwork.RoleAccess.GetAllAsync(
                    ra => ra.UserCode == indb.UserCode,
                    includeProperties: "User,UserRole")).ToList();

                if (inDbRoles.Any(r => r.UserRole.RoleLevel >= roleInClaim.RoleLevel))
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NoAccess, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return BadRequest(new { data = data.EncryptedData });
                }

                await _iunitofwork.User.ActiveDeactiveUser(indb.UserCode);

                var updatedUser = await _iunitofwork.User.FirstOrDefaultAsync(d => d.UserCode == indb.UserCode);
                var userRoles = (await _iunitofwork.RoleAccess.GetAllAsync(
                    ra => ra.UserCode == updatedUser.UserCode,
                    includeProperties: "UserRole"))
                    .OrderByDescending(x => x.UserRole.RoleLevel)
                    .ToList();

                if (!updatedUser.IsActive)
                {
                    foreach (var userRole in userRoles)
                    {
                        await _iunitofwork.RoleAccess.UpdateAsync(userRole.AccessId, async entity =>
                        {
                            entity.AccessToRole = false;
                            await Task.CompletedTask;
                        });
                    }
                }
                else
                {
                    await _iunitofwork.RoleAccess.UpdateAsync(userRoles.First().AccessId, async entity =>
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

        [HttpPost("useraccessofroles")]
        [Authorize(Policy = SD.IsAccess)]
        public async Task<IActionResult> GetUserAccessOfRoles([FromBody] EncryptedDataVM details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, details.Data);
                if (decryptedData == null)
                {
                    var response = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", response.Hash);
                    return BadRequest(new { data = response.EncryptedData });
                }

                var stringValueVM = JsonSerializer.Deserialize<StringValueVM>(decryptedData);
                if (string.IsNullOrEmpty(stringValueVM.Value))
                {
                    var response = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", response.Hash);
                    return BadRequest(new { data = response.EncryptedData });
                }

                var rolesInDb = await _iunitofwork.RoleAccess.GetAllAsync(
                    u => u.UserCode == stringValueVM.Value && u.AccessToRole == true,
                    includeProperties: "User,UserRole");

                var successResponse = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, rolesInDb)));
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

        [HttpPost("updateroleaccess")]
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
                var userRoleInClaim = await _iunitofwork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == claimRoleCode);

                if (userRoleInClaim == null)
                {
                    var encryptedResponse = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NoAccess, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", encryptedResponse.Hash);
                    return BadRequest(new { data = encryptedResponse.EncryptedData });
                }

                foreach (var userAndRole in userAndRoles)
                {
                    var requiredRole = await _iunitofwork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == userAndRole.RoleCode);

                    if (requiredRole == null ||
                        (requiredRole.RoleLevel != RoleLevels.SUPREME && requiredRole.RoleLevel >= userRoleInClaim.RoleLevel))
                    {
                        var encryptedResponse = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NoAccess, null)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", encryptedResponse.Hash);
                        return BadRequest(new { data = encryptedResponse.EncryptedData });
                    }

                    var userAndRoleInDb = await _iunitofwork.RoleAccess.FirstOrDefaultAsync(
                        x => x.RoleCode == userAndRole.RoleCode && x.UserCode == userAndRole.UserCode);

                    if (userAndRoleInDb == null)
                    {
                        RoleAccess addUserAndRole = new RoleAccess
                        {
                            AccessId = _iunitofwork.RoleAccess.GenrateUniqueCode(),
                            UserCode = userAndRole.UserCode,
                            RoleCode = userAndRole.RoleCode,
                            AccessToRole = true
                        };
                        await _iunitofwork.RoleAccess.AddAsync(addUserAndRole);
                    }
                    else if (userAndRoleInDb.AccessToRole != userAndRole.AccessToRole)
                    {
                        await _iunitofwork.RoleAccess.UpdateAsync(userAndRoleInDb.AccessId, async entity =>
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
        [HttpPost("updateuser")]
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
                var userInClaim = await _iunitofwork.User
                    .FirstOrDefaultAsync(d => d.UserCode == User.FindFirst(ClaimTypes.SerialNumber).Value);

                var roleInClaim = await _iunitofwork.UserRole
                    .FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                if (user.UserCode != userInClaim.UserCode)
                {
                    var indb = await _iunitofwork.User.FirstOrDefaultAsync(d => d.UserCode == user.UserCode);
                    if (indb == null)
                    {
                        var resp = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, ValidationMessages.NotFound)));

                        HttpContext.Response.Headers.Append("X-Data-Hash", resp.Hash);
                        return NotFound(new { data = resp.EncryptedData });
                    }

                    var inDbRoles = (await _iunitofwork.RoleAccess.GetAllAsync(
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

                    await _iunitofwork.User.UpdateAsync(user.UserCode, async entity =>
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
                    await _iunitofwork.User.UpdateAsync(user.UserCode, async entity =>
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
