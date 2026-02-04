using API.Data.IRepositories;
using API.Model.UserModels;
using API.Data.IRepository;
using API.Resources;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using API.Helpers;
using API.Helpers.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Forms;
using API.Model.ViewModels.TransporterManagementViewModels;

namespace API.Controllers.UserControllers
{
    [ApiController]
    [Route(SD.baseUrl + "userrole")]
    [Authorize(Policy = SD.IsAccess)]
    public class UserRoleController : Controller
    {
        private readonly IUnitofWork _iunitofwork;
        private readonly IEncryptionHelper _encryptionHelper;

        public UserRoleController(IUnitofWork iunitofwork, IEncryptionHelper encryptionHelper)
        {
            _iunitofwork = iunitofwork;
            _encryptionHelper = encryptionHelper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
               
                var userRoleInClaim = await _iunitofwork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));

                if (userRoleInClaim.RoleLevel == RoleLevels.SUPREME || userRoleInClaim.RoleLevel == RoleLevels.ADMIN)
                {
                    var userRoles = await _iunitofwork.UserRole.GetAllAsync();
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                  _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, userRoles)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return Ok(new { data = data.EncryptedData });
                }
                else
                {
                    var userRoles = await _iunitofwork.UserRole.GetAllAsync(d => d.RoleLevel < userRoleInClaim.RoleLevel);
                    var encryptedData = _encryptionHelper.Encrypt(JsonSerializer.Serialize(userRoles));

                    HttpContext.Response.Headers.Append("X-Data-Hash", encryptedData.Hash);
                    return Ok(new { data = encryptedData.EncryptedData });
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
        [HttpPost("role")]
        public async Task<IActionResult> GetRole([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    // Decrypt the data into a VM
                    StringValueVM roleCodeVM = JsonSerializer.Deserialize<StringValueVM>(decryptedData); // Create RoleCodeVM with "string roleCode"

                    if (TryValidateModel(roleCodeVM))
                    {

                        var userRoleInClaim = await _iunitofwork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                        var requiredRole = await _iunitofwork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == roleCodeVM.Value);

                        if (requiredRole == null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                                _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, ValidationMessages.NotFound)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return NotFound(new { data = data.EncryptedData });
                        }

                        if (userRoleInClaim.RoleLevel != RoleLevels.SUPREME && requiredRole.RoleLevel >= userRoleInClaim.RoleLevel)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                                _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, ValidationMessages.NoAccess)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }

                        var encryptedResponse = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, requiredRole)));

                        HttpContext.Response.Headers.Append("X-Data-Hash", encryptedResponse.Hash);
                        return Ok(new { data = encryptedResponse.EncryptedData });
                    }
                    else
                    {
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest,
                                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList())));

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
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));

                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new { data = data.EncryptedData });
            }
        }

        [HttpPost("create")]

        public async Task<IActionResult> CreateRole([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    UserRole userRoles = JsonSerializer.Deserialize<UserRole>(decryptedData);

                    if (TryValidateModel(userRoles))
                    {
                        var indb = await _iunitofwork.UserRole.FirstOrDefaultAsync(x => x.RoleName == userRoles.RoleName);
                        if (indb != null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                                _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, ValidationMessages.Exists)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }

                        var supremeLevelRole = await _iunitofwork.UserRole.FirstOrDefaultAsync(d => d.RoleLevel == RoleLevels.SUPREME);
                        if (supremeLevelRole != null && userRoles.RoleLevel == RoleLevels.SUPREME)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                                _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NoAccess, ValidationMessages.NoAccess)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }

                        UserRole role = new UserRole()
                        {
                            RoleCode = _iunitofwork.UserRole.GenrateUniqueCode(),
                            RoleName = userRoles.RoleName,
                            RoleLevel = userRoles.RoleLevel,
                            RoleType = userRoles.RoleType,
                        };

                        await _iunitofwork.UserRole.AddAsync(role);

                        var response = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Created, ValidationMessages.Created)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", response.Hash);
                        return Ok(new { data = response.EncryptedData });
                    }
                    else
                    {
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest,
                                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList())));
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
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new { data = data.EncryptedData });
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateRole([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    UserRole userRole = JsonSerializer.Deserialize<UserRole>(decryptedData);

                    if (TryValidateModel(userRole))
                    {
                        var indb = await _iunitofwork.UserRole.GetAsync(userRole.RoleCode);

                        if (indb == null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                                _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, ValidationMessages.NotFound)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return NotFound(new { data = data.EncryptedData });
                        }

                        var indbExists = await _iunitofwork.UserRole.FirstOrDefaultAsync(x => x.RoleName == userRole.RoleName && x.Id != indb.Id);
                        if (indbExists != null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                                _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, ValidationMessages.Exists)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }

                        var supremeLevelRole = await _iunitofwork.UserRole.FirstOrDefaultAsync(d => d.RoleLevel == RoleLevels.SUPREME);
                        if (supremeLevelRole != null && userRole.RoleLevel == RoleLevels.SUPREME && userRole.RoleCode != supremeLevelRole.RoleCode)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                                _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NoAccess, ValidationMessages.NoAccess)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }

                        await _iunitofwork.UserRole.UpdateAsync(indb.RoleCode, async entity =>
                        {
                            entity.RoleName = userRole.RoleName;
                            entity.RoleLevel = userRole.RoleLevel;
                            entity.RoleType = userRole.RoleType;
                            await Task.CompletedTask;
                        });

                        var successData = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Updated, ValidationMessages.Updated)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", successData.Hash);
                        return Ok(new { data = successData.EncryptedData });
                    }
                    else
                    {
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest,
                                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList())));
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
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new { data = data.EncryptedData });
            }
        }
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteRole([FromBody] EncryptedDataVM Details)
        {   
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);
                if (decryptedData != null)
                {

                    StringValueVM userRole = JsonSerializer.Deserialize<StringValueVM>(decryptedData);
                    if (TryValidateModel(userRole))
                    {
                        var indb = await _iunitofwork.UserRole.GetAsync(userRole.Value);
                        if (indb != null)
                        {
                            var usersshave = await _iunitofwork.RoleAccess.FirstOrDefaultAsync(d => d.RoleCode == indb.RoleCode);
                            var menushave = await _iunitofwork.MenuAccess.FirstOrDefaultAsync(d => d.RoleCode == indb.RoleCode && d.Status == true);
                            var routesshave = await _iunitofwork.RouteAccess.FirstOrDefaultAsync(d => d.RoleCode == indb.RoleCode && d.Status == true);
                            if (usersshave != null || menushave != null || routesshave != null)
                            {
                                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                                                 _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.ObjectDepends, null)));
                                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                                return BadRequest(new { data = data.EncryptedData });
                            }
                            else
                            {

                                await _iunitofwork.UserRole.RemoveAsync(indb);
                                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                                                   _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                                return NotFound(new { data = data.EncryptedData });
                            }
                              
                        }
                        else
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                                                   _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return NotFound(new { data = data.EncryptedData });
                        }

                    }
                    else
                    {
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest,
                                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList())));
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
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new { data = data.EncryptedData });
            }
        }

    }
}
