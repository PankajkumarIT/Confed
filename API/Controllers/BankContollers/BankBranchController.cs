using API.Data.IRepositories;
using API.Helpers.Models;
using API.Helpers;
using API.Model.ManagementModels.BankManagement;
using API.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using API.Model.ViewModels.TransporterManagementViewModels;
using API.Model.UserModels;

namespace API.Controllers.BankContollers
{
    [ApiController]
    [Route(SD.baseUrl + "bankbranch")]
    [Authorize(Policy = SD.IsAccess)]
    public class BankBranchController : ControllerBase
    {
        private readonly IUnitofWork _unitofWork;
        private readonly IEncryptionHelper _encryptionHelper;

        public BankBranchController(IUnitofWork unitofWork, IEncryptionHelper encryptionHelper)
        {
            _unitofWork = unitofWork;
            _encryptionHelper = encryptionHelper;
        }
        [HttpGet("getAll")]

        public async Task<IActionResult> GetAllBankBranch()

        {

            try

            {

                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                if (userInClaim.IsEntityUser == true)

                {

                    var organizationclaimcode = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == User.FindFirst(SD.OrganizationCode).Value);

                    var banks = await _unitofWork.BankBranch.GetAllAsync(x => x.OrganizationCode == organizationclaimcode.OrganizationCode, includeProperties: "Organization");

                    var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(

                        _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, banks)));

                    HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);

                    return Ok(new { data = okdata.EncryptedData });

                }

                else

                {

                    var banks = await _unitofWork.BankBranch.GetAllAsync(includeProperties: "Organization");

                    var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(

                        _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, banks)));

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

        [HttpPost("getbankbranchByOrgCode")]
        public async Task<IActionResult> GetbyBankbranchByOrgode([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    StringValueVM user = JsonSerializer.Deserialize<StringValueVM>(decryptedData);
                    if (TryValidateModel(user))
                    {
                        var departmnetindb = await _unitofWork.BankBranch.GetAllAsync(x => x.OrganizationCode == user.Value);
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
                   (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
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

        [HttpPost("getbyBankcode")]
        public async Task<IActionResult> GetbyBankcode([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    StringValueVM user = JsonSerializer.Deserialize<StringValueVM>(decryptedData);
                    if (TryValidateModel(user))
                    {
                        var departmnetindb = await _unitofWork.BankBranch.FirstOrDefaultAsync(x => x.BankBranchCode == user.Value);
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
                   (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
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
        [HttpPost("addBank")]
        public async Task<IActionResult> AddBankBranch([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    BankBranch vm = JsonSerializer.Deserialize<BankBranch>(decryptedData);
                    if (TryValidateModel(vm))
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


                        var bankindb = await _unitofWork.BankBranch.FirstOrDefaultAsync(x => x.BankBranchCode == vm.BankBranchCode || x.IFSC == vm.IFSC || x.MICR == vm.MICR);
                        if (bankindb != null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }
                        else
                        {
                            var getstorage = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == vm.OrganizationCode);
                            double storageleft = getstorage.UsedStorageSize + vm.TotalStorageSize;
                            if (getstorage.StorageSize < storageleft)
                            {
                                var okdatsa = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.failure, ResponseHandler.NoStorage, null)));
                                HttpContext.Response.Headers.Append("X-Data-Hash", okdatsa.Hash);
                                return BadRequest(new { data = okdatsa.EncryptedData });
                            }
                            var bank = new BankBranch
                            {
                                BankBranchCode = _unitofWork.BankBranch.GenrateUniqueCode(),
                                BankName = vm.BankName,
                                BranchName = vm.BranchName,
                                IFSC = vm.IFSC,
                                MICR = vm.MICR,
                                ContactPerson = vm.ContactPerson,
                                ContactNumber = vm.ContactNumber,
                                Email = vm.Email,
                                IsActive = true,
                                BranchAddress = vm.BranchAddress,
                                OrganizationCode = vm.OrganizationCode,
                                TotalStorageSize = vm.TotalStorageSize,
                            };

                            await _unitofWork.BankBranch.AddAsync(bank);
                            await _unitofWork.Organization.UpdateAsync(getstorage.OrganizationCode, async entity =>
                            {
                                entity.AllocateStorageSize += vm.TotalStorageSize;
                                await Task.CompletedTask;
                            });
                        }


                        var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Created, vm)));
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
        [HttpPost("updateBank")]
        public async Task<IActionResult> UpdateDepartment([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    BankBranch bankBranch = JsonSerializer.Deserialize<BankBranch>(decryptedData);
                    if (TryValidateModel(bankBranch))
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


                        var bankBranchindb = await _unitofWork.BankBranch.FirstOrDefaultAsync(x => x.BankBranchCode == bankBranch.BankBranchCode, includeProperties: "Organization");
                        if (bankBranchindb == null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return Ok(new { data = data.EncryptedData });
                        }
                        else
                        {
                            double storageleft = bankBranchindb.Organization.UsedStorageSize + bankBranch.TotalStorageSize;
                            if (bankBranchindb.Organization.StorageSize < storageleft)
                            {
                                var okdatsa = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.failure, ResponseHandler.NoStorage, null)));
                                HttpContext.Response.Headers.Append("X-Data-Hash", okdatsa.Hash);
                                return BadRequest(new { data = okdatsa.EncryptedData });
                            }
                            await _unitofWork.BankBranch.UpdateAsync(bankBranch.BankBranchCode, async entity =>
                            {
                                entity.BankName = bankBranch.BankName;
                                entity.BranchName = bankBranch.BranchName;
                                entity.IFSC = bankBranch.IFSC;
                                entity.MICR = bankBranch.MICR;
                                entity.ContactPerson = bankBranch.ContactPerson;
                                entity.BranchAddress = bankBranch.BranchAddress;
                                entity.ContactNumber = bankBranch.ContactNumber;
                                entity.Email = bankBranch.Email;
                                entity.OrganizationCode = bankBranch.OrganizationCode;
                                entity.TotalStorageSize = bankBranch.TotalStorageSize;

                                await Task.CompletedTask;
                            });
                            await _unitofWork.Organization.UpdateAsync(bankBranch.OrganizationCode, async entity =>
                            {
                                entity.AllocateStorageSize = (entity.AllocateStorageSize) - bankBranchindb.TotalStorageSize + bankBranch.TotalStorageSize;
                                await Task.CompletedTask;
                            });

                        }
                        var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Created, bankBranch)));
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
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteBankBranch([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);
                if (decryptedData == null)
                {
                    var bad = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", bad.Hash);
                    return BadRequest(new { data = bad.EncryptedData });
                }

                var req = JsonSerializer.Deserialize<StringValueVM>(decryptedData);
                if (req == null || string.IsNullOrEmpty(req.Value))
                {
                    var bad = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", bad.Hash);
                    return BadRequest(new { data = bad.EncryptedData });
                }

                var existing = await _unitofWork.BankBranch.FirstOrDefaultAsync(b => b.BankBranchCode == req.Value);
                if (existing == null)
                {
                    var nf = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", nf.Hash);
                    return NotFound(new { data = nf.EncryptedData });
                }
                var bankuserindb = await _unitofWork.BankUser.FirstOrDefaultAsync(x => x.BankBranchCode == req.Value);
                if (bankuserindb != null)
                {
                    var nf = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                      _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.ObjectDepends, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", nf.Hash);
                    return BadRequest(new { data = nf.EncryptedData });
                }
                await _unitofWork.BankBranch.RemoveAsync(existing);

                var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Deleted, null)));
                HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                return Ok(new { data = okdata.EncryptedData });
            }
            catch (Exception ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new { data = data.EncryptedData });
            }
        }
    }
}
