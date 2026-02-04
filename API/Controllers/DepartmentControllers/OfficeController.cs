using API.Data;
using API.Data.IRepositories;
using API.Helpers;
using API.Helpers.Models;
using API.Model.ManagementModels.DepartmentManagement;
using API.Model.ManagementModels.UserModels;
using API.Model.UserModels;
using API.Model.ViewModels.TransporterManagementViewModels;
using API.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPOI.HPSF;
using System.Security.Claims;
using System.Text.Json;

namespace API.Controllers.DepartmentControllers
{
    [ApiController]

    [Route(SD.baseUrl + "Office")]
    [Authorize(Policy = SD.IsAccess)]
    public class OfficeController : ControllerBase
    {
        private readonly IUnitofWork _unitofWork;
        private readonly ApplicationDbContext _context;
        private readonly IEncryptionHelper _encryptionHelper;
        public OfficeController(ApplicationDbContext dbContext, IUnitofWork unitofWork, IEncryptionHelper encryptionHelper)
        {
            _context = dbContext;
            _unitofWork = unitofWork;
            _encryptionHelper = encryptionHelper;
        }
        [HttpGet("getAllOffice")]
        public async Task<IActionResult> GetAllOffice()
        {
            try
            {
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                if (userInClaim.IsEntityUser == true)
                {
                    var organizationclaimcode = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == User.FindFirst(SD.OrganizationCode).Value);

                    var organizationibdb = await _unitofWork.Office.GetAllAsync(x => x.OrganizationCode == organizationclaimcode.OrganizationCode, includeProperties: "Organization");
                    var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                  (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Sent, organizationibdb)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                    return Ok(new { data = okdata.EncryptedData });
                }
                else
                {
                    var organizationibdb = await _unitofWork.Office.GetAllAsync(includeProperties: "Organization");
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
        [HttpPost("addOffice")]
        public async Task<IActionResult> AddOffice([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    Office user = JsonSerializer.Deserialize<Office>(decryptedData);
                    if (TryValidateModel(user))
                    {
                        var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                        var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                        if (userRoleInClaim.RoleLevel != RoleLevels.SUPREME && userRoleInClaim.RoleLevel != RoleLevels.ADMIN&& userRoleInClaim.RoleLevel != RoleLevels.AUTHORITY)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }
                  
                        var Officeindb = await _unitofWork.Office.FirstOrDefaultAsync(x => x.OfficeCode == user.OfficeCode);
                        if (Officeindb != null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }
                        else
                        {
                   
                            Office dept = new Office
                            {
                                OfficeCode = _unitofWork.Office.GenrateUniqueCode(),
                                OfficeName = user.OfficeName,
                                IsActive = user.IsActive,
                                ContactEmail = user.ContactEmail,
                                ContactNumber = user.ContactNumber,
                                OfficeAddress = user.OfficeAddress,
                                ContactPersonName = user.ContactPersonName,
                                OrganizationCode = user.OrganizationCode,
                                TotalStorageSize = user.TotalStorageSize,
                            };
                            await _unitofWork.Office.AddAsync(dept);
                            await _unitofWork.Organization.UpdateAsync(user.OrganizationCode, async entity =>
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
        [HttpPost("updateOffice")]
        public async Task<IActionResult> UpdateOffice([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    Office user = JsonSerializer.Deserialize<Office>(decryptedData);
                    if (TryValidateModel(user))
                    {
                        var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                        var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                        if (userRoleInClaim.RoleLevel != RoleLevels.SUPREME && userRoleInClaim.RoleLevel != RoleLevels.ADMIN && userRoleInClaim.RoleLevel != RoleLevels.AUTHORITY)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }

                 
                        var Officeindb = await _unitofWork.Office.FirstOrDefaultAsync(x => x.OfficeCode == user.OfficeCode , includeProperties : "Organization");
                        if (Officeindb == null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return Ok(new { data = data.EncryptedData });
                        }
                        else
                        {
                            await _unitofWork.Office.UpdateAsync(user.OfficeCode, async entity =>
                            {
                                entity.OfficeName = user.OfficeName;
                                entity.IsActive = user.IsActive;
                                entity.ContactEmail = user.ContactEmail;
                                entity.ContactPersonName = user.ContactPersonName;
                                entity.ContactNumber = user.ContactNumber;
                                entity.OfficeAddress = user.OfficeAddress;
                                entity.OrganizationCode = user.OrganizationCode;
                                entity.TotalStorageSize = user.TotalStorageSize;    
                                await Task.CompletedTask;
                            });
                            await _unitofWork.Organization.UpdateAsync(Officeindb.Organization.OrganizationCode, async entity =>
                            {
                               entity.AllocateStorageSize = (entity.AllocateStorageSize) - Officeindb.TotalStorageSize + user.TotalStorageSize;
                                await Task.CompletedTask;
                            });

                        }


                        var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Updated, user)));
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
        [HttpPost("getByOfficecode")]
        public async Task<IActionResult> GetByOfficecode([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    StringValueVM user = JsonSerializer.Deserialize<StringValueVM>(decryptedData);
                    if (TryValidateModel(user))
                    {
                        var departmnetindb = await _unitofWork.Office.FirstOrDefaultAsync(x => x.OfficeCode == user.Value);
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
                        var Officeindb = await _unitofWork.Office.FirstOrDefaultAsync(x => x.OfficeCode == user.Value);
                        if (Officeindb == null)
                        {
                            var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                           (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                            return NotFound(new { data = okdata.EncryptedData });
                        }
                        else
                        {
                            var Officeuserindb = await _unitofWork.DepartmentUser.FirstOrDefaultAsync(x => x.OfficeCode == user.Value);
                            if (Officeuserindb != null)
                            {
                                var nf = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                                  _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.ObjectDepends, null)));
                                HttpContext.Response.Headers.Append("X-Data-Hash", nf.Hash);
                                return BadRequest(new { data = nf.EncryptedData });
                            }
                            else
                            {
                                await _unitofWork.Office.RemoveAsync(Officeindb);
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
