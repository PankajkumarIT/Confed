using API.Data.IRepositories;
using API.Model.Routes;
using API.Data.IRepository;
using API.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Route = API.Model.Routes.Route;
using API.Helpers;
using API.Data.Repositories;
using API.Helpers.Models;
using System.Text.Json;
using API.Model.ViewModels.TransporterManagementViewModels;
using Microsoft.AspNetCore.Components.Forms;

namespace API.Controllers.UserControllers
{
    [ApiController]
    [Route(SD.baseUrl + "routeaccess")]
    [Authorize(Policy = SD.IsAccess)]
    public class RouteAccessController : ControllerBase
    {
        private readonly IUnitofWork _iunitofwork;
        private readonly IEncryptionHelper _encryptionHelper;


        public RouteAccessController(IUnitofWork repository, IEncryptionHelper encryptionHelper)
        {
            _iunitofwork = repository;
            _encryptionHelper = encryptionHelper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRouteAccess()
        {
            try
            {
                var list = await _iunitofwork.RouteAccess.GetAllAsync(includeProperties: "UserRole,Route");
                var jsonData = JsonSerializer.Serialize(
                    _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, list)
                );


                var encryptedResult = _encryptionHelper.Encrypt(jsonData);


                HttpContext.Response.Headers.Append("X-Data-Hash", encryptedResult.Hash);


                return Ok(new { data = encryptedResult.EncryptedData });
            }
            catch (Exception ex)
            {
                var errorJson = JsonSerializer.Serialize(
                    _encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, ex.Message)
                );

                var encryptedError = _encryptionHelper.Encrypt(errorJson);
                HttpContext.Response.Headers.Append("X-Data-Hash", encryptedError.Hash);

                return StatusCode(500, new { data = encryptedError.EncryptedData });
            }
        }

        [HttpPost("getaccessedroutes")]
        public async Task<IActionResult> GetAccessedRoutesWithRole([FromBody] EncryptedDataVM details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, details.Data);

                if (decryptedData != null)
                {

                    StringValueVM roleCodeVMCode = JsonSerializer.Deserialize<StringValueVM>(decryptedData);
                    if (TryValidateModel(roleCodeVMCode))
                    {

                        var allMappedRoutes = (await _iunitofwork.RouteAccess.GetAllAsync(
                            d => d.RoleCode == roleCodeVMCode.Value && d.Status == true && d.Route.Status == true,
                            includeProperties: "UserRole,Route")).ToList();

                        List<Route> allRoutes = new List<Route>();
                        foreach (var mappedRoute in allMappedRoutes)
                        {
                            allRoutes.Add(mappedRoute.Route);
                        }

                        var RoutesAndSubRoutes = allRoutes
                            .Where(Route => Route.ParentCode == null)
                            .Select(mainRoute => _iunitofwork.Route.GetRouteWithSubRoutes(mainRoute, allRoutes))
                            .ToList();

                        var encryptedResult = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, RoutesAndSubRoutes)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", encryptedResult.Hash);
                        return Ok(new { data = encryptedResult.EncryptedData });
                    }
                    else
                    {
                        var errorData = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest,
                            ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList())));
                        HttpContext.Response.Headers.Append("X-Data-Hash", errorData.Hash);
                        return BadRequest(new { data = errorData.EncryptedData });
                    }
                }
                else
                {
                    var badData = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", badData.Hash);
                    return BadRequest(new { data = badData.EncryptedData });
                }
            }
            catch (Exception ex)
            {
                var errorData = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", errorData.Hash);
                return StatusCode(500, new { data = errorData.EncryptedData });
            }
        }

        [HttpPost("createandupdate")]
        public async Task<IActionResult> CreateAndUpdateRouteAccess([FromBody] EncryptedDataVM details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, details.Data);

                if (decryptedData != null)
                {
                    RouteAccess[] routeAccesses = JsonSerializer.Deserialize<RouteAccess[]>(decryptedData);

                    if (routeAccesses != null)
                    {
                        foreach (var routeAccess in routeAccesses)
                        {
                            var InDb = await _iunitofwork.RouteAccess.FirstOrDefaultAsync(x =>
                                x.RouteCode == routeAccess.RouteCode &&
                                x.RoleCode == routeAccess.RoleCode);

                            if (InDb == null)
                            {
                                RouteAccess mapping = new RouteAccess()
                                {
                                    AccessCode = _iunitofwork.RouteAccess.GenrateUniqueCode(),
                                    RouteCode = routeAccess.RouteCode,
                                    RoleCode = routeAccess.RoleCode,
                                    Status = true
                                };
                                await _iunitofwork.RouteAccess.AddAsync(mapping);
                            }
                            else if (InDb.Status != routeAccess.Status)
                            {
                                await _iunitofwork.RouteAccess.UpdateAsync(InDb.AccessCode, async entity =>
                                {
                                    entity.Status = routeAccess.Status;
                                    await Task.CompletedTask;
                                });
                            }
                        }

                        var responseData = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, ValidationMessages.Ok)));
                        HttpContext.Response.Headers.Append("X-Data-Hash", responseData.Hash);
                        return Ok(new { data = responseData.EncryptedData });
                    }
                    else
                    {
                        var badData = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                            _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, "Invalid or empty data")));
                        HttpContext.Response.Headers.Append("X-Data-Hash", badData.Hash);
                        return BadRequest(new { data = badData.EncryptedData });
                    }
                }
                else
                {
                    var badData = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", badData.Hash);
                    return BadRequest(new { data = badData.EncryptedData });
                }
            }
            catch (Exception ex)
            {
                var errorData = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", errorData.Hash);
                return StatusCode(500, new { data = errorData.EncryptedData });
            }
        }
    }
}
