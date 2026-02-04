using API.Data.IRepositories;
using API.Helpers;
using API.Helpers.Models;

using API.Model.Menus;
using API.Model.ViewModels.TransporterManagementViewModels;
using API.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Route = API.Model.Routes.Route;


namespace API.Controllers.UserControllers
{
    [ApiController]
    [Authorize(Policy = SD.IsAccess)]
    [Route(SD.baseUrl + "route")]
    public class RouteController : Controller
    {
        private readonly IUnitofWork _iunitofwork;
        private readonly IEncryptionHelper _encryptionHelper;
        public RouteController(IUnitofWork repository,IEncryptionHelper encryptionHelper)
        {
            _iunitofwork = repository;
            _encryptionHelper = encryptionHelper;
        }

        [HttpGet]
        public async Task<IActionResult> GetMenus()
        {
            try
            {
                var allMenus = await _iunitofwork.Route.GetAllAsync();

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

        [HttpPost("getroutesandsubroutes")]
        public async Task<IActionResult> GetRoutesAndSubRoutes([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var dercryptdata = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);
                var allMenus = (await _iunitofwork.Route.GetAllAsync(x => x.Status == true)).ToList();

                if (dercryptdata != null)
                {
                    StringValueVM routeByparentCodeVM = JsonSerializer.Deserialize<StringValueVM>(dercryptdata);
                    if (routeByparentCodeVM.Value == null)
                    {
                        var result = allMenus
                            .Where(menu => menu.ParentCode == null)
                            .Select(mainMenu => _iunitofwork.Route.GetRouteWithSubRoutes(mainMenu, allMenus))
                            .ToList();
                        var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                          (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, result)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                        return Ok(new { data = data.EncryptedData });
                    }
                    else
                    {
                        var result = allMenus
                       .Where(menu => menu.ParentCode == routeByparentCodeVM.Value)
                       .Select(mainMenu => _iunitofwork.Route.GetRouteWithSubRoutes(mainMenu, allMenus))
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
                        .Select(mainMenu => _iunitofwork.Route.GetRouteWithSubRoutes(mainMenu, allMenus))
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

        [HttpPost("route")]
        public async Task<IActionResult> GetRoute([FromBody] EncryptedDataVM details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, details.Data);

                if (decryptedData != null)
                {
                    StringValueVM routecodeVM = JsonSerializer.Deserialize<StringValueVM>(decryptedData);

                    if (TryValidateModel(routecodeVM))
                    {
                        var indb = await _iunitofwork.Route.FirstOrDefaultAsync(x => x.RouteCode == routecodeVM.Value);

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
        public async Task<IActionResult> CreateRoute([FromBody] EncryptedDataVM details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, details.Data);

                if (decryptedData != null)
                {
                    Route Route = JsonSerializer.Deserialize<Route>(decryptedData);

                    if (TryValidateModel(Route))
                    {
                        var indb = await _iunitofwork.Route.FirstOrDefaultAsync(x =>
                            (x.Path == Route.Path || x.RouteName == Route.RouteName) && x.ParentCode == Route.ParentCode);

                        if (indb != null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                                _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }

                        Route addRoute = new Route()
                        {
                            RouteCode = _iunitofwork.Route.GenrateUniqueCode(),
                            RouteName = Route.RouteName,
                            Path = Route.Path,
                            ParentCode = Route.ParentCode,
                            Status = true
                        };

                        await _iunitofwork.Route.AddAsync(addRoute);

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
        public async Task<IActionResult> UpdateRoute([FromBody] EncryptedDataVM details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, details.Data);

                if (decryptedData != null)
                {
                    Route route = JsonSerializer.Deserialize<Route>(decryptedData);

                    if (TryValidateModel(route))
                    {
                        var indb = await _iunitofwork.Route.FirstOrDefaultAsync(x => x.RouteCode == route.RouteCode);
                        if (indb == null)
                        {
                            var dataNotFound = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                                _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", dataNotFound.Hash);
                            return NotFound(new { data = dataNotFound.EncryptedData });
                        }

                        var indbExist = await _iunitofwork.Route.FirstOrDefaultAsync(x =>
                            (x.Path == route.Path || x.RouteName == route.RouteName) &&
                            x.ParentCode == route.ParentCode &&
                            x.RouteCode != indb.RouteCode);

                        if (indbExist != null)
                        {
                            var dataExists = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                                _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", dataExists.Hash);
                            return BadRequest(new { data = dataExists.EncryptedData });
                        }

                        await _iunitofwork.Route.UpdateAsync(indb.RouteCode, async entity =>
                        {
                            entity.RouteName = route.RouteName;
                            entity.Path = route.Path;
                            entity.Status = route.Status;
                            entity.ParentCode = route.ParentCode;
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
        public async Task<IActionResult> DeleteRoute([FromBody] EncryptedDataVM details)
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
                            _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, "InvalId RouteCode")));
                        HttpContext.Response.Headers.Append("X-Data-Hash", dataInvalId.Hash);
                        return BadRequest(new { data = dataInvalId.EncryptedData });
                    }

                    var indb = await _iunitofwork.Route.FirstOrDefaultAsync(x => x.RouteCode == ViewModel.Value);
                    if (indb == null)
                    {
                        var dataNotFound = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", dataNotFound.Hash);
                        return NotFound(new { data = dataNotFound.EncryptedData });
                    }

                    var propshave = await _iunitofwork.Route.FirstOrDefaultAsync(d => d.RouteCode == indb.RouteCode);
                    var roleshave = await _iunitofwork.RouteAccess.FirstOrDefaultAsync(d => d.RouteCode == indb.RouteCode && d.Status == true);

                    if (propshave == null && roleshave == null)
                    {
                        var dataDepend = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.ObjectDepends, null)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", dataDepend.Hash);
                        return BadRequest(new { data = dataDepend.EncryptedData });
                    }

                    await _iunitofwork.Route.RemoveAsync(indb.RouteCode);

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
