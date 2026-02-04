using API.Data.IRepositories;
using API.Model.AreaModels;
using API.Data.IRepository;
using API.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.Helpers;
using System.Text.Json;
using API.Helpers.Models;
using API.Model.ViewModels.TransporterManagementViewModels;

namespace API.Controllers.AreaControllers
{
    [ApiController]
    [Route(SD.baseUrl + "ctv")]
    [Authorize(Policy = SD.IsAccess)]
    public class CtvController : ControllerBase
    {
        private readonly IUnitofWork _iunitofwork;
        private readonly IEncryptionHelper _encryptionHelper;
        public CtvController(IUnitofWork repository, IEncryptionHelper encryptionHelper)
        {
            _iunitofwork = repository;
            _encryptionHelper = encryptionHelper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var records = await _iunitofwork.Ctv.GetAllAsync(includeProperties: "District.State");
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

        [HttpPost("ctv")]
        public async Task<IActionResult> GetCTV([FromBody] EncryptedDataVM Details)
        {

            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    StringValueVM ctvCodeVM = JsonSerializer.Deserialize<StringValueVM>(decryptedData);

                    if (TryValidateModel(ctvCodeVM))
                    {
                        var indb = await _iunitofwork.Ctv.GetAsync(ctvCodeVM.Value);
                        if (indb != null)
                        {
                            var allCtvs = await _iunitofwork.Ctv.GetAllAsync(includeProperties: "District.State");
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, allCtvs)));
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


        [HttpPost("ctvsbydistrict")]
        public async Task<IActionResult> GetCTV_By_DistCode([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    StringValueVM stringValueVM = JsonSerializer.Deserialize<StringValueVM>(decryptedData);


                    if (TryValidateModel(stringValueVM))
                    {
                        var indb = await _iunitofwork.Ctv.GetAllAsync(x => x.DistrictCode == stringValueVM.Value, includeProperties: "District.State");
                        if (indb == null)
                        {

                            var allctv = await _iunitofwork.Ctv.GetAllAsync();
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, allctv)));
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
        public async Task<IActionResult> CreateCTV([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {
                    Ctv ctv = JsonSerializer.Deserialize<Ctv>(decryptedData);

                    if (TryValidateModel(ctv))
                    {

                        var indb = await _iunitofwork.Ctv.FirstOrDefaultAsync(x => x.CtvLGDCode == ctv.CtvLGDCode || x.CtvName == ctv.CtvName);
                        if (indb != null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }
                        else
                        {
                            Ctv addCtv = new Ctv()
                            {
                                CtvCode = _iunitofwork.Ctv.GenrateUniqueCode(),
                                CtvLGDCode = ctv.CtvLGDCode,
                                CtvName = ctv.CtvName,
                                DistrictCode = ctv.DistrictCode
                            };
                            await _iunitofwork.Ctv.AddAsync(addCtv);
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

        public async Task<IActionResult> UpdateCTV([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {

                    Ctv ctv = JsonSerializer.Deserialize<Ctv>(decryptedData);
                    if (TryValidateModel(ctv))
                    {

                        var indb = await _iunitofwork.Ctv.GetAsync(ctv.CtvCode);
                        if (indb == null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                               (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return NotFound(new { data = data.EncryptedData });
                        }

                        var indbExists = await _iunitofwork.Ctv.FirstOrDefaultAsync(x => (x.CtvLGDCode == ctv.CtvLGDCode || x.CtvName == ctv.CtvName) && x.CtvCode != indb.CtvCode);

                        if (indbExists != null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                            (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }
                        else
                        {
                            await _iunitofwork.Ctv.UpdateAsync(ctv.CtvCode, async entity =>
                            {
                                entity.CtvName = ctv.CtvName;
                                entity.CtvLGDCode = ctv.CtvLGDCode;
                                entity.DistrictCode = ctv.DistrictCode;
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
        public async Task<IActionResult> DeleteCTV([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData != null)
                {

                    StringValueVM ViewModel = JsonSerializer.Deserialize<StringValueVM>(decryptedData);
                    if (TryValidateModel(ViewModel))
                    {

                        var indb = await _iunitofwork.Ctv.GetAsync(ViewModel.Value);
                        var userInd = await _iunitofwork.User.FirstOrDefaultAsync(x => x.CtvCode == indb.CtvCode);

                        if (userInd != null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                            (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }
                        else
                        {

                            await _iunitofwork.Ctv.RemoveAsync(indb);
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
