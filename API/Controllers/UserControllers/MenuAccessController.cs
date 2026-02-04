using API.Data.IRepositories;
using API.Data.Repositories;
using API.Helpers;
using API.Helpers.Models;
using API.Model.Menus;
using API.Model.ViewModels.TransporterManagementViewModels;
using API.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace API.Controllers.UserControllers
{
    [ApiController]
    [Route(SD.baseUrl + "menuaccess")]
    [Authorize(Policy = SD.IsAccess)]
    public class MenuAccessController : ControllerBase
    {
        private readonly IUnitofWork _iunitofwork;
        private readonly IEncryptionHelper _encryptionHelper;
        public MenuAccessController(IUnitofWork repository, IEncryptionHelper encryptionHelper)
        {
            _iunitofwork = repository;
            _encryptionHelper = encryptionHelper;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllMappingMenus()
        {
            try
            {
                var list = await _iunitofwork.MenuAccess.GetAllAsync(includeProperties: "UserRole,Menu");
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
               (_encryptionHelper.GenrateResponse(false, StatusType.success, ResponseHandler.NotFound, list)));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return Ok(new { data = data.EncryptedData });
            }
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                (_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new { data = data.EncryptedData });
            }
        }

        [HttpPost("getAccessedMenus")]
        public async Task<IActionResult> GetAccessedMenusWithRole([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decrpytdata = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);
                if (decrpytdata != null)
                {
                    StringValueVM userRole = JsonSerializer.Deserialize<StringValueVM>(decrpytdata);
                    if (TryValidateModel(userRole))
                    {
                        var allMappedMenus = await _iunitofwork.MenuAccess.GetAllAsync(d => d.RoleCode == userRole.Value && d.Status == true && d.Menu.Status == true, includeProperties: "UserRole,Menu");
                        List<Menu> allMenus = new List<Menu>();
                        foreach (var mappedMenu in allMappedMenus)
                        {
                            var menu = mappedMenu.Menu;
                            allMenus.Add(menu);
                        }
                        var menusAndSubMenus = allMenus
                            .Where(menu => menu.ParentCode == null)
                            .Select(mainMenu => _iunitofwork.Menu.GetMenuWithSubmenus(mainMenu, allMenus))
                            .ToList();
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                        (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, menusAndSubMenus)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                        return Ok(new { data = data.EncryptedData });
                    }
                    else
                    {
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                      (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                        return NotFound(new { data = data.EncryptedData });
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
                return StatusCode(500, new { data = data.EncryptedData });
            }
        }

        [HttpPost("createandupdate")]
        public async Task<IActionResult> CreateAndUpdateMappingMenus([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptdata = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);
                if (decryptdata != null)
                {
                    MenuAccess[] mappingMenus = JsonSerializer.Deserialize<MenuAccess[]>(decryptdata);
                    if (TryValidateModel(mappingMenus))
                    {

                        foreach (var menu in mappingMenus)
                        {
                            var menuInDb = await _iunitofwork.MenuAccess.FirstOrDefaultAsync(x => x.MenuCode == menu.MenuCode && x.RoleCode == menu.RoleCode);

                            if (menuInDb == null)
                            {
                                MenuAccess mapping = new MenuAccess()
                                {
                                    AccessCode = _iunitofwork.MenuAccess.GenrateUniqueCode(),
                                    MenuCode = menu.MenuCode,
                                    RoleCode = menu.RoleCode,
                                    Status = true
                                };
                                await _iunitofwork.MenuAccess.AddAsync(mapping);
                            }
                            else if (menuInDb.Status != menu.Status)
                            {
                                await _iunitofwork.MenuAccess.UpdateAsync(menuInDb.AccessCode, async entity =>
                                {
                                    entity.Status = menu.Status;
                                    await Task.CompletedTask;
                                });
                            }
                        }
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                        (_encryptionHelper.GenrateResponse(true, StatusType.failure, ResponseHandler.Ok, mappingMenus)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                        return Ok(new { data = data.EncryptedData });
                    }
                    else
                    {
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                            (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
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
                return StatusCode(500, new { data = data.EncryptedData });
            }
        }
    }
}


