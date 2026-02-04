using API.Data;
using API.Data.IRepositories;
using API.Helpers;
using API.Helpers.Models;
using API.Model.ManagementModels.DepartmentManagement;
using API.Model.UserModels;
using API.Model.ViewModels.TransporterManagementViewModels;
using API.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace API.Controllers.DepartmentControllers
{
    [ApiController]
    [Route(SD.baseUrl + "Designation")]
    [Authorize(Policy = SD.IsAccess)]
    public class DesignationController : ControllerBase
    {
        private readonly IUnitofWork _unitofWork;
        private readonly ApplicationDbContext _context;
        private readonly IEncryptionHelper _encryptionHelper;
        public DesignationController(ApplicationDbContext dbContext, IUnitofWork unitofWork, IEncryptionHelper encryptionHelper)
        {
            _context = dbContext;
            _unitofWork = unitofWork;
            _encryptionHelper = encryptionHelper;
        }
        [HttpGet("getAllDesignation")]
        public async Task<IActionResult> GetAllDesignation()
        {
            try
            {
                var organizationibdb = await _unitofWork.Designation.GetAllAsync();
                var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Sent, organizationibdb)));
                HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                return Ok(new { data = okdata.EncryptedData });

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
        [HttpPost("addDesignation")]
        public async Task<IActionResult> AddDesignation([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    Designation user = JsonSerializer.Deserialize<Designation>(decryptedData);
                    if (TryValidateModel(user))
                    {
                        var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                        var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                        if (userRoleInClaim.RoleLevel != RoleLevels.SUPREME && userRoleInClaim.RoleLevel != RoleLevels.ADMIN)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return Ok(new { data = data.EncryptedData });
                        }


                        var Designationindb = await _unitofWork.Designation.FirstOrDefaultAsync(x => x.DesignationCode == user.DesignationCode );
                        if (Designationindb != null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }
                        else
                        {
                            Designation dept = new Designation
                            {
                                DesignationCode = _unitofWork.Designation.GenrateUniqueCode(),
                                DesignationName = user.DesignationName,
                                IsActive = user.IsActive,
                            };

                            await _unitofWork.Designation.AddAsync(dept);



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
        [HttpPost("updateDesignation")]
        public async Task<IActionResult> UpdateDesignation([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    Designation user = JsonSerializer.Deserialize<Designation>(decryptedData);
                    if (TryValidateModel(user))
                    {
                        var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                        var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                        if (userRoleInClaim.RoleLevel != RoleLevels.SUPREME && userRoleInClaim.RoleLevel != RoleLevels.ADMIN)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return Ok(new { data = data.EncryptedData });
                        }


                        var Designationindb = await _unitofWork.Designation.FirstOrDefaultAsync(x => x.DesignationCode == user.DesignationCode);
                        if (Designationindb == null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return Ok(new { data = data.EncryptedData });
                        }
                        else
                        {
                            await _unitofWork.Designation.UpdateAsync(user.DesignationCode, async entity =>
                            {
                                entity.DesignationName = user.DesignationName;
                                entity.IsActive = user.IsActive;
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
        [HttpPost("getByDesignationcode")]
        public async Task<IActionResult> GetByDesignationcode([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    StringValueVM user = JsonSerializer.Deserialize<StringValueVM>(decryptedData);
                    if (TryValidateModel(user))
                    {
                        var departmnetindb = await _unitofWork.Designation.FirstOrDefaultAsync(x => x.DesignationCode == user.Value);
                        if (departmnetindb == null)
                        {
                            var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                           (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                            return NotFound(new { data = okdata.EncryptedData });
                        }
                        else
                        {
                            var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                           (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, departmnetindb)));
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
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    StringValueVM user = JsonSerializer.Deserialize<StringValueVM>(decryptedData);
                    if (TryValidateModel(user))
                    {
                        var Designationindb = await _unitofWork.Designation.FirstOrDefaultAsync(x => x.DesignationCode == user.Value);
                        if (Designationindb == null)
                        {
                            var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                           (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                            return NotFound(new { data = okdata.EncryptedData });
                        }
                        else
                        {
                            var Designationuserindb = await _unitofWork.DepartmentUser.FirstOrDefaultAsync(x => x.DesignationCode == user.Value);
                            if (Designationuserindb != null)
                            {
                                var nf = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                                  _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.ObjectDepends, null)));
                                HttpContext.Response.Headers.Append("X-Data-Hash", nf.Hash);
                                return BadRequest(new { data = nf.EncryptedData });
                            }
                            else
                            {
                                await _unitofWork.Designation.RemoveAsync(Designationindb);
                                var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                               (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Deleted, null)));
                                HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                                return Ok(new { data = okdata.EncryptedData });
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
                return StatusCode(500, new
                {
                    data = data.EncryptedData
                });
            }

        }
    }
}
