
using API.Data.IRepositories;
using API.Helpers;
using API.Helpers.Models;
using API.Model.Menus;
using API.Model.QueryParamViewModels;
using API.Model.ViewModels.TransporterManagementViewModels;
using API.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace API.Controllers.UserControllers
{
    [ApiController]
    [Authorize(Policy = SD.IsAccess)]
    [Route(SD.baseUrl + "menu")]
    public class MenuController : Controller
    {
        private readonly IUnitofWork _iunitofwork;
        private readonly IEncryptionHelper _encryptionHelper;

        public MenuController(IUnitofWork repository, IEncryptionHelper encryptionHelper)
        {
            _iunitofwork = repository;
            _encryptionHelper = encryptionHelper;
        }

        [HttpGet]
        public async Task<IActionResult> GetMenus()
        {
            try
            {
                var allMenus = await _iunitofwork.Menu.GetAllAsync();

                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, allMenus)
                ));

                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);

                return Ok(new { data = data.EncryptedData });
            }
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)
                ));

                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);

                return StatusCode(500, new { data = data.EncryptedData });
            }

        }
        [HttpPost("getmenusandsubmenus")]
        public async Task<IActionResult> GetMenusAndSubMenus([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var dercryptdata = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);
                var allMenus = (await _iunitofwork.Menu.GetAllAsync(x => x.Status == true)).ToList();

                if (dercryptdata != null)
                {
                    StringValueVM menuByparentCodeVM = JsonSerializer.Deserialize<StringValueVM>(dercryptdata);
                    if (menuByparentCodeVM.Value == null)
                        {
                            var result = allMenus
                                .Where(menu => menu.ParentCode == null)
                                .Select(mainMenu => _iunitofwork.Menu.GetMenuWithSubmenus(mainMenu, allMenus))
                                .ToList();
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                          (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, result)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                        return Ok(new { data = data.EncryptedData });
                    }
                    else
                    {
                            var result = allMenus
                           .Where(menu => menu.ParentCode == menuByparentCodeVM.Value)
                           .Select(mainMenu => _iunitofwork.Menu.GetMenuWithSubmenus(mainMenu, allMenus))
                           .ToList();

                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                          (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, result)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return Ok(new { data = data.EncryptedData });

                    }
                }
                else
                {
                    var result = allMenus
                        .Where(menu => menu.ParentCode == null)
                        .Select(mainMenu => _iunitofwork.Menu.GetMenuWithSubmenus(mainMenu, allMenus))
                        .ToList();
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                    (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, result)));
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
        [HttpPost("menu")]
        public async Task<IActionResult> GetMenuBymenuCode([FromBody] EncryptedDataVM details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, details.Data);

                if (decryptedData != null)
                {
                    StringValueVM menuCodeVM = JsonSerializer.Deserialize<StringValueVM>(decryptedData);

                    if (TryValidateModel(menuCodeVM))
                    {
                        var indb = await _iunitofwork.Menu.FirstOrDefaultAsync(x => x.MenuCode == menuCodeVM.Value);

                        if (indb == null)
                        {
                            var dataNF = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                                _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", dataNF.Hash);
                            return NotFound(new { data = dataNF.EncryptedData });
                        }

                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, indb)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                        return Ok(new { data = data.EncryptedData });
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
            catch (Exception ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new { data = data.EncryptedData });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateMenu([FromBody] EncryptedDataVM details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, details.Data);

                if (decryptedData != null)
                {
                    Menu menu = JsonSerializer.Deserialize<Menu>(decryptedData);

                    if (TryValidateModel(menu))
                    {
                        var indb = await _iunitofwork.Menu.FirstOrDefaultAsync(x =>
                            (x.Path == menu.Path || x.MenuName == menu.MenuName ) && x.ParentCode == menu.ParentCode);

                        if (indb != null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                                _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }

                        Menu addMenu = new Menu()
                        {
                            MenuCode = _iunitofwork.Menu.GenrateUniqueCode(),
                            MenuName = menu.MenuName,
                            Path = menu.Path,
                            Icon = menu.Icon,
                            ParentCode = menu.ParentCode,
                            Status = true,
                            SequenceNumber =menu.SequenceNumber,
                        };

                        await _iunitofwork.Menu.AddAsync(addMenu);

                        var dataSuccess = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Created, null)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", dataSuccess.Hash);
                        return Ok(new { data = dataSuccess.EncryptedData });
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
            catch (Exception ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new { data = data.EncryptedData });
            }
        }


        [HttpPost("update")]
        public async Task<IActionResult> UpdateMenu([FromBody] EncryptedDataVM details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, details.Data);

                if (decryptedData != null)
                {
                    Menu menu = JsonSerializer.Deserialize<Menu>(decryptedData);

                    if (TryValidateModel(menu))
                    {
                        var indb = await _iunitofwork.Menu.FirstOrDefaultAsync(x => x.MenuCode == menu.MenuCode);
                        if (indb == null)
                        {
                            var dataNotFound = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                                _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", dataNotFound.Hash);
                            return NotFound(new { data = dataNotFound.EncryptedData });
                        }

                        var indbExist = await _iunitofwork.Menu.FirstOrDefaultAsync(x =>
                            (x.Path == menu.Path || x.MenuName == menu.MenuName ) &&
                            x.ParentCode == menu.ParentCode &&
                            x.MenuCode != indb.MenuCode);

                        if (indbExist != null)
                        {
                            var dataExists = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                                _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", dataExists.Hash);
                            return BadRequest(new { data = dataExists.EncryptedData });
                        }

                        await _iunitofwork.Menu.UpdateAsync(indb.MenuCode, async entity =>
                        {
                            entity.MenuName = menu.MenuName;
                            entity.Path = menu.Path;
                            entity.Status = menu.Status;
                            entity.SequenceNumber = menu.SequenceNumber;
                            entity.Icon = menu.Icon;
                            entity.ParentCode = menu.ParentCode;
                            await Task.CompletedTask;
                        });

                        var dataSuccess = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Updated, null)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", dataSuccess.Hash);
                        return Ok(new { data = dataSuccess.EncryptedData });
                    }
                    else
                    {
                        var dataInvalId = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest,
                                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList())));
                        HttpContext.Response.Headers.Append("X-Data-Hash", dataInvalId.Hash);
                        return BadRequest(new { data = dataInvalId.EncryptedData });
                    }
                }
                else
                {
                    var dataHashFail = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", dataHashFail.Hash);
                    return BadRequest(new { data = dataHashFail.EncryptedData });
                }
            }
            catch (Exception ex)
            {
                var dataError = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", dataError.Hash);
                return StatusCode(500, new { data = dataError.EncryptedData });
            }
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteMenu([FromBody] EncryptedDataVM details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, details.Data);

                if (decryptedData != null)
                {
                    StringValueVM ViewModel = JsonSerializer.Deserialize<StringValueVM>(decryptedData);

                    if (string.IsNullOrEmpty(ViewModel.Value))
                    {
                        var dataInvalId = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, "InvalId menuCode")));
                        HttpContext.Response.Headers.Append("X-Data-Hash", dataInvalId.Hash);
                        return BadRequest(new { data = dataInvalId.EncryptedData });
                    }

                    var indb = await _iunitofwork.Menu.FirstOrDefaultAsync(x => x.MenuCode == ViewModel.Value);
                    if (indb == null)
                    {
                        var dataNotFound = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", dataNotFound.Hash);
                        return NotFound(new { data = dataNotFound.EncryptedData });
                    }

                    var propshave = await _iunitofwork.Menu.FirstOrDefaultAsync(d => d.MenuCode == indb.MenuCode);
               var roleshave = await _iunitofwork.MenuAccess.FirstOrDefaultAsync(d => d.MenuCode == indb.MenuCode && d.Status == true);

            if (propshave == null && roleshave == null)
                    {
                        var dataDepend = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.ObjectDepends, null)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", dataDepend.Hash);
                        return BadRequest(new { data = dataDepend.EncryptedData });
                    }

                    await _iunitofwork.Menu.RemoveAsync(indb.MenuCode);

                    var dataDeleted = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Deleted, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", dataDeleted.Hash);
                    return Ok(new { data = dataDeleted.EncryptedData });
                }
                else
                {
                    var dataInvalId = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", dataInvalId.Hash);
                    return BadRequest(new { data = dataInvalId.EncryptedData });
                }
            }
            catch (Exception Ex)
            {
                var dataError = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", dataError.Hash);
                return StatusCode(500, new { data = dataError.EncryptedData });
            }
        }
    }
}
