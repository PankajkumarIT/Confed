
using API.Data.IRepositories;
using API.Helpers;
using API.Helpers.Models;
using API.Model.AreaModels;
using API.Model.ViewModels.TransporterManagementViewModels;
using API.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace API.Controllers.AreaControllers
{
    [Route(SD.baseUrl + "district")]
    [ApiController]
    [Authorize(Policy = SD.IsAccess)]
    public class DistrictController : Controller
    {
        private readonly IUnitofWork _iunitofwork;
        private readonly IEncryptionHelper _encryptionHelper;
        public DistrictController(IUnitofWork repository, IEncryptionHelper encryptionHelper)
        {
            _iunitofwork = repository;
            _encryptionHelper = encryptionHelper;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var records = await _iunitofwork.District.GetAllAsync(includeProperties: "State");
                if (records == null)
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                        (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return NotFound(new { data = data.EncryptedData });
                }
                else
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                        (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, records)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return Ok(new { data = data.EncryptedData });
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

        [HttpPost("district")]
        public async Task<IActionResult> GetDistrict_By_DistCode([FromBody] EncryptedDataVM Details)
        {

            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    StringValueVM districtCodeVM = JsonSerializer.Deserialize<StringValueVM>(decryptedData);

                    if (TryValidateModel(districtCodeVM))
                    {
                        var indb = await _iunitofwork.District.GetAsync(districtCodeVM.Value);

                        if (indb == null)
                        {
                            var allDistrict = await _iunitofwork.District.GetAllAsync();
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, allDistrict)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return Ok(new { data = data.EncryptedData });
                        }
                        else
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, indb)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return Ok(new { data = data.EncryptedData });
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

        [HttpPost("districtsbystate")]
        public async Task<IActionResult> GetDistrict_By_stateCode([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    StringValueVM districtCodeVM = JsonSerializer.Deserialize<StringValueVM>(decryptedData);

                    if (TryValidateModel(districtCodeVM))
                    {
                        var indb = await _iunitofwork.District.GetAllAsync(x => x.StateCode == districtCodeVM.Value);
                        if (indb == null)
                        {

                            var allDistrict = await _iunitofwork.District.GetAllAsync();
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, allDistrict)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return Ok(new { data = data.EncryptedData });
                        }
                        else
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, indb)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return Ok(new { data = data.EncryptedData });
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

        [HttpPost("create")]
        public async Task<IActionResult> CreateDistrict([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    District district = JsonSerializer.Deserialize<District>(decryptedData);

                    if (TryValidateModel(district))
                    {

                        var indb = await _iunitofwork.District.FirstOrDefaultAsync(x => x.DistrictLGDCode == district.DistrictLGDCode || x.DistrictName == district.DistrictName);
                        if (indb != null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }
                        else
                        {
                            District addDistrict = new District()
                            {

                                DistrictCode = _iunitofwork.District.GenrateUniqueCode(),
                                DistrictName = district.DistrictName,
                                DistrictLGDCode = district.DistrictLGDCode,
                                StateCode = district.StateCode,
                            };
                            await _iunitofwork.District.AddAsync(addDistrict);
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Created, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return Ok(new { data = data.EncryptedData });
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

        [HttpPost("update")]
        public async Task<IActionResult> UpdateDistrict([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {

                    District district = JsonSerializer.Deserialize<District>(decryptedData);
                    if (TryValidateModel(district))
                    {

                        var indb = await _iunitofwork.District.GetAsync(district.DistrictCode);
                        if (indb == null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                               (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return NotFound(new { data = data.EncryptedData });
                        }

                        var indbExists = await _iunitofwork.District.FirstOrDefaultAsync(x => (x.DistrictLGDCode == district.DistrictLGDCode || x.DistrictName == district.DistrictName) && x.DistrictCode != indb.DistrictCode);

                        if (indbExists != null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                            (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }
                        else
                        {
                            await _iunitofwork.District.UpdateAsync(indb.DistrictCode, async entity =>
                            {
                                entity.DistrictLGDCode = district.DistrictLGDCode;
                                entity.DistrictName = district.DistrictName;
                                entity.StateCode = district.StateCode;
                                await Task.CompletedTask;
                            });
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Updated, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return Ok(new { data = data.EncryptedData });
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

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteDistrict([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {

                    StringValueVM ViewModel = JsonSerializer.Deserialize<StringValueVM>(decryptedData);

                    if (TryValidateModel(ViewModel))
                    {

                        var indb = await _iunitofwork.District.GetAsync(ViewModel.Value);
                        if (indb == null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                            (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return NotFound(new { data = data.EncryptedData });
                        }
                        else
                        {
                            //var propshave = await _iunitofwork.Ctv.FirstOrDefaultAsync(d => d.districtCode == indb.districtCode);
                            //if (propshave != null)
                            //    return BadRequest(new { message = ValIdationMessages.ObjectDepends });

                            await _iunitofwork.District.RemoveAsync(indb);
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                            (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Deleted, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return Ok(new { data = data.EncryptedData });
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





    }
}
