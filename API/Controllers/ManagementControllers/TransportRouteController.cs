using API.Data.IRepositories;
using API.Data.Repositories;
using API.Helpers;
using API.Helpers.Models;
using API.Model.ManagementModels.DepartmentManagement;
using API.Model.ManagementModels.TransporterManagement;
using API.Model.ManagementModels.UserModels;
using API.Model.QueryParamViewModels;
using API.Model.UserModels;
using API.Model.ViewModels.TransporterManagementViewModels;
using API.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPOI.POIFS.Properties;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Security.Claims;
using System.Text.Json;

namespace API.Controllers.ManagementControllers
{
    [ApiController]
    [Route(SD.baseUrl + "transportroute")]
    [Authorize(Policy = SD.IsAccess)]
    public class TransportRouteController : ControllerBase
    {
        private readonly IUnitofWork _unitofWork;
        private readonly IEncryptionHelper _encryptionHelper;
        public TransportRouteController(IUnitofWork unitofWork, IEncryptionHelper encryptionHelper)
        {
            _unitofWork = unitofWork;
            _encryptionHelper = encryptionHelper;
        }
        [HttpGet("getAllTransportRoute")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x =>x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                var routeVMList = new List<GetTransportRouteVM>();
                List<TransportRoute> transportRoutes = new List<TransportRoute>();

                if (userInClaim.IsEntityUser)
                {
                    var organizationClaim = await _unitofWork.Organization.FirstOrDefaultAsync(x =>x.OrganizationCode == User.FindFirst(SD.OrganizationCode).Value);

                    var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d =>d.RoleCode == User.FindFirstValue(ClaimTypes.Role));

                    var userontranportdb = await _unitofWork.AdministorUser.FirstOrDefaultAsync(x => x.UserCode == userInClaim.UserCode);

                    if (userontranportdb != null)
                    {
                        transportRoutes = (await _unitofWork.TransportRoute.GetAllAsync(x =>x.SourceOrganizationCode == organizationClaim.OrganizationCode,includeProperties: "Vehicle.VehicleType,Vehicle.Organization")).ToList();

                    }
                    else
                    {
                        var latestHistories = (await _unitofWork.TransportRouteHistory.GetAllAsync(h => h.ActionByUserCode == userInClaim.UserCode && h.ActionByRoleCode == userRoleInClaim.RoleCode)).GroupBy(x => x.TransportRouteCode).ToList();

                        var codes = latestHistories.Select(h => h.Key).Distinct().ToList();


                        transportRoutes = (await _unitofWork.TransportRoute.GetAllAsync(x =>codes.Contains(x.TransportRouteCode) && x.SourceOrganizationCode == organizationClaim.OrganizationCode,includeProperties: "Vehicle.VehicleType,Vehicle.Organization")).ToList();
                    }


                }
                else
                {
                    transportRoutes = (await _unitofWork.TransportRoute.GetAllAsync(

                        includeProperties: "Vehicle.VehicleType,Vehicle.Organization")).ToList();
                }

                foreach (var route in transportRoutes)
                {
                    var driverlist = await _unitofWork.VehicleDriverMap.GetAllAsync(x => x.VehicleCode == route.VehicleCode, includeProperties: "Vehicle,DriverDetail.User");
                    var gatepassindb = await _unitofWork.GatePass.FirstOrDefaultAsync(h => h.TransportRouteCode == route.TransportRouteCode);
                    var historyList = await _unitofWork.TransportRouteHistory.GetAllAsync(h => h.TransportRouteCode == route.TransportRouteCode);
                    var getSourceOrganization = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == route.SourceOrganizationCode);
                    var historyVM = new List<TransportRouteHistoryVM>();
                    foreach (var h in historyList)
                    {
                        var actionByUser = await _unitofWork.User.FirstOrDefaultAsync(u => u.UserCode == h.ActionByUserCode);

                        var actionByRole = await _unitofWork.UserRole.FirstOrDefaultAsync(r => r.RoleCode == h.ActionByRoleCode);

                        var assignedUser = await _unitofWork.User.FirstOrDefaultAsync(u => u.UserCode == h.AssignedToUserCode);

                        var assignedRole = await _unitofWork.UserRole.FirstOrDefaultAsync(r => r.RoleCode == h.AssignedToRoleCode);
                        historyVM.Add(new TransportRouteHistoryVM
                        {
                            ApprovalCode = h.ApprovalCode,
                            TransportRouteCode = h.TransportRouteCode,
                            TransportRoute = route,
                            ActionByUserCode = h.ActionByUserCode,
                            ActionByUser = actionByUser,
                            ActionByRoleCode = h.ActionByRoleCode,
                            ActionByRole = actionByRole,
                            AssignedToUserCode = h.AssignedToUserCode,
                            AssignedToUser = assignedUser,
                            AssignedToRoleCode = h.AssignedToRoleCode,
                            AssignedToRole = assignedRole,
                            Status = h.Status,
                            Remarks = h.Remarks,
                            ActionDate = h.ActionDate
                        });
                    }
                    routeVMList.Add(new GetTransportRouteVM
                    {
                        TransportRouteCode = route.TransportRouteCode,
                        PickupAddress = route.PickupAddress,
                        SourceOrganization = route.SourceOrganizationCode,
                        Organization = getSourceOrganization ?? null,
                        DestinationAddress = route.DestinationAddress,
                        DestinationContactNo = route.DestinationContactNo,
                        VehicleCode = route.VehicleCode,
                        Vehicle = route.Vehicle,
                        Quantity = route.Quantity,
                        DriverList = driverlist?.ToList() ?? new List<VehicleDriverMap>(),
                        TotalWeight = route.TotalWeight,
                        DistanceInKm = route.DistanceInKm,
                        ExpectedTravelTimeHours = route.ExpectedTravelTimeHours,
                        ExpectedJourneyStart = route.ExpectedJourneyStart,
                        ExpectedJourneyEnd = route.ExpectedJourneyEnd,
                        ActualJourneyStart = route.ActualJourneyStart,
                        ActualJourneyEnd = route.ActualJourneyEnd,
                        BaseAmount = route.BaseAmount,
                        Commodities = route.Commodities == null
                        ? null
                        : JsonSerializer.Deserialize<List<Commodity>>(route.Commodities),

                        TollTax = route.TollTax,
                        OtherCharges = route.OtherCharges,
                        TotalCharge = route.TotalCharge,
                        ApprovalStatus = route.ApprovalStatus,
                        DestinationStatus = route.DestinationStatus,
                        IsCompleted = route.IsCompleted,
                        TransportRouteHistoryVM = historyVM,
                        GatePass = gatepassindb ?? null,
                        CreatedBy = route.CreatedBy,
                        CreatedDate = route.CreatedDate,
                        UpdatedBy = route.UpdatedBy,
                        UpdatedDate = route.UpdatedDate,
                    });
                }

                var encryptedResponse = _encryptionHelper.Encrypt(
                    JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, routeVMList)));

                HttpContext.Response.Headers.Append("X-Data-Hash", encryptedResponse.Hash);
                return Ok(new { data = encryptedResponse.EncryptedData });
            }
            catch (Exception ex)
            {
                var errorResponse = _encryptionHelper.Encrypt(
                    JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, ex.Message)));

                HttpContext.Response.Headers.Append("X-Data-Hash", errorResponse.Hash);
                return StatusCode(500, new { data = errorResponse.EncryptedData });
            }
        }
        [HttpGet("PendingTransportRouteByRole")]
        public async Task<IActionResult> PendingTransportRouteByRole()
        {
            try
            {
                var userCode = User.FindFirstValue(ClaimTypes.SerialNumber);
                var roleCode = User.FindFirstValue(ClaimTypes.Role);
                var orgCode = User.FindFirst(SD.OrganizationCode)?.Value;
                var user = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == userCode);
                var routeVMList = new List<GetTransportRouteVM>();
                var role = await _unitofWork.UserRole.FirstOrDefaultAsync(r => r.RoleCode == roleCode);
                List<TransportRoute> routes = new List<TransportRoute>();
                if (user.IsEntityUser)
                {
                    var userontranportdb = await _unitofWork.AdministorUser.FirstOrDefaultAsync(x => x.UserCode == userCode);

                    if (userontranportdb !=null)
                    {
                        routes = (await _unitofWork.TransportRoute.GetAllAsync(x =>
                            x.SourceOrganizationCode == orgCode && x.ApprovalStatus == "Pending",
                                includeProperties: "Vehicle.VehicleType,Vehicle.Organization")).ToList();
                    }
                    else
                    {
                        var latestHistories = (await _unitofWork.TransportRouteHistory.GetAllAsync()).GroupBy(x => x.TransportRouteCode)
                      .Select(g => g.OrderByDescending(x => x.Id).First())
                      .ToList();

                        var history = latestHistories
                             .Where(h =>
                                 h.AssignedToUserCode == userCode &&
                                 h.AssignedToRoleCode == roleCode
                             )
                             .ToList();
                        var codes = history.Select(h => h.TransportRouteCode).Distinct().ToList();

                        routes = (await _unitofWork.TransportRoute.GetAllAsync(x =>
                            codes.Contains(x.TransportRouteCode) &&
                            x.SourceOrganizationCode == orgCode &&
                            x.ApprovalStatus == "Pending",
                                includeProperties: "Vehicle.VehicleType,Vehicle.Organization")).ToList();
                    }
                }
                else
                {
                    routes = (await _unitofWork.TransportRoute.GetAllAsync(x =>
                        x.ApprovalStatus == "Pending",
                                includeProperties: "Vehicle.VehicleType,Vehicle.Organization")).ToList();
                }
                foreach (var r in routes)
                {
                    var historyList = await _unitofWork.TransportRouteHistory
                        .GetAllAsync(h => h.TransportRouteCode == r.TransportRouteCode);
                    var driverlist = await _unitofWork.VehicleDriverMap.GetAllAsync(x => x.VehicleCode == r.VehicleCode, includeProperties: "Vehicle,DriverDetail.User");

                    var historyVmList = new List<TransportRouteHistoryVM>();

                    foreach (var h in historyList)
                    {
                        var actionUser = await _unitofWork.User.FirstOrDefaultAsync(u => u.UserCode == h.ActionByUserCode);
                        var actionRole = await _unitofWork.UserRole.FirstOrDefaultAsync(r => r.RoleCode == h.ActionByRoleCode);
                        var assignUser = await _unitofWork.User.FirstOrDefaultAsync(u => u.UserCode == h.AssignedToUserCode);
                        var assignRole = await _unitofWork.UserRole.FirstOrDefaultAsync(r => r.RoleCode == h.AssignedToRoleCode);

                        historyVmList.Add(new TransportRouteHistoryVM
                        {
                            ApprovalCode = h.ApprovalCode,
                            TransportRouteCode = h.TransportRouteCode,
                            TransportRoute = h.TransportRoute,
                            ActionByUserCode = h.ActionByUserCode,
                            ActionByRoleCode = h.ActionByRoleCode,
                            AssignedToUserCode = h.AssignedToUserCode,
                            AssignedToRoleCode = h.AssignedToRoleCode,
                            Status = h.Status,
                            Remarks = h.Remarks,
                            ActionDate = h.ActionDate,
                            ActionByUser = actionUser,
                            ActionByRole = actionRole,
                            AssignedToUser = assignUser,
                            AssignedToRole = assignRole
                        });
                    }

                    routeVMList.Add(new GetTransportRouteVM
                    {
                        TransportRouteCode = r.TransportRouteCode,
                        TotalWeight = r.TotalWeight,
                        TollTax = r.TollTax,
                        DestinationStatus = r.DestinationStatus,
                        BaseAmount = r.BaseAmount,
                        ActualJourneyEnd = r.ActualJourneyEnd,
                        ExpectedJourneyStart = r.ExpectedJourneyStart,
                        ExpectedJourneyEnd = r.ExpectedJourneyEnd,
                        OtherCharges = r.OtherCharges,
                        ExpectedTravelTimeHours = r.ExpectedTravelTimeHours,
                        Vehicle = r.Vehicle,
                        SourceOrganization = r.SourceOrganizationCode,

                        DestinationAddress = r.DestinationAddress,
                        DestinationContactNo = r.DestinationContactNo,
                        DriverList = driverlist?.ToList() ?? new List<VehicleDriverMap>(),
                        VehicleCode = r.VehicleCode,
                        Quantity = r.Quantity,
                        DistanceInKm = r.DistanceInKm,
                        TotalCharge = r.TotalCharge,
                        Commodities = r.Commodities == null
                        ? null
                        : JsonSerializer.Deserialize<List<Commodity>>(r.Commodities),
                        PickupAddress =r.PickupAddress,
                        ApprovalStatus = r.ApprovalStatus,
                        IsCompleted = r.IsCompleted,
                        TransportRouteHistoryVM = historyVmList,
                        CreatedBy = r.CreatedBy,
                        CreatedDate = r.CreatedDate,
                        UpdatedBy = r.UpdatedBy,
                        UpdatedDate = r.UpdatedDate,
                    });
                }

                var encrypted = _encryptionHelper.Encrypt(
                    JsonSerializer.Serialize(
                        _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, routeVMList)));

                HttpContext.Response.Headers.Append("X-Data-Hash", encrypted.Hash);
                return Ok(new { data = encrypted.EncryptedData });
            }
            catch (Exception ex)
            {
                var encrypted = _encryptionHelper.Encrypt(
                    JsonSerializer.Serialize(
                        _encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, ex.Message)));

                HttpContext.Response.Headers.Append("X-Data-Hash", encrypted.Hash);
                return StatusCode(500, new { data = encrypted.EncryptedData });
            }
        }
        [HttpPost("addTransportRouteDetail")]
        public async Task<IActionResult> AddTranportRoute([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);
                if (decryptedData != null)
                {
                    TransportRouteVM request = JsonSerializer.Deserialize<TransportRouteVM>(decryptedData);
                    if (TryValidateModel(request))
                    {
                        var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                        var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                        var organizationclaimcode = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == User.FindFirst(SD.OrganizationCode).Value);
                        var transporrouteindb = await _unitofWork.TransportRoute.FirstOrDefaultAsync(x => x.TransportRouteCode == request.TransportRouteCode);
                        if (transporrouteindb != null)
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                              (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Exists, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return BadRequest(new { data = data.EncryptedData });
                        }
                        else
                        {
                            TransportRoute transportRoute = new TransportRoute()
                            {
                                TransportRouteCode = _unitofWork.TransportRoute.GenrateUniqueCode(),
                                SourceOrganizationCode = organizationclaimcode.OrganizationCode,
                                DestinationAddress = request.DestinationAddress,
                                PickupAddress = request.PickupAddress,
                                DestinationContactNo = request.DestinationContactNo,
                                VehicleCode = request.VehicleCode,
                                Quantity = request.Quantity,
                                TotalWeight = request.TotalWeight,
                                DistanceInKm = request.DistanceInKm,
                                ExpectedTravelTimeHours = request.ExpectedTravelTimeHours,
                                ExpectedJourneyStart = request.ExpectedJourneyStart,
                                ExpectedJourneyEnd = request.ExpectedJourneyEnd,
                                BaseAmount = request.BaseAmount,
                                TollTax = request.TollTax,
                                OtherCharges = request.OtherCharges,
                                Commodities = System.Text.Json.JsonSerializer.Serialize(request.Commodities),
                                TotalCharge = request.TotalCharge,
                                ApprovalStatus = "Pending",
                                IsCompleted = false,
                                UserCode = userInClaim.UserCode,
                                DestinationStatus = null,
                                CreatedBy = userInClaim.Name,
                                CreatedDate = DateTime.Now.ToLocalTime(),

                            };
                            if (userRoleInClaim.RoleLevel == RoleLevels.AUTHORITY)
                            {
                                transportRoute.ApprovalStatus = "Approved";
                                transportRoute.DestinationStatus = "Dispatched";
                                transportRoute.IsCompleted = true;
                            }
                            await _unitofWork.TransportRoute.AddAsync(transportRoute);

                            var getallroleindb = await _unitofWork.UserRole.GetAllAsync(x => x.RoleType == "department");
                            if ((userRoleInClaim.RoleLevel == RoleLevels.INTERMEDIATE && userRoleInClaim.RoleType == "department") ||
                                (userRoleInClaim.RoleLevel == RoleLevels.AUTHORITY && userRoleInClaim.RoleType == "department")
                                )
                            {

                                TransportRouteHistory routeApprovalHistory = new TransportRouteHistory()
                                {
                                    ApprovalCode = _unitofWork.TransportRoute.GenrateUniqueCode(),
                                    TransportRouteCode = transportRoute.TransportRouteCode,
                                    ActionByUserCode = userInClaim.UserCode,
                                    ActionByRoleCode = userRoleInClaim.RoleCode,
                                    AssignedToRoleCode = null,
                                    AssignedToUserCode = null,
                                    ActionDate = DateTime.Now.ToLocalTime(),
                                    Status = "Approved",
                                };
                                await _unitofWork.TransportRouteHistory.AddAsync(routeApprovalHistory);

                            }
                            else
                            {

                                TransportRouteHistory routeApprovalHistory = new TransportRouteHistory()
                                {
                                    ApprovalCode = _unitofWork.TransportRoute.GenrateUniqueCode(),
                                    TransportRouteCode = transportRoute.TransportRouteCode,
                                    ActionByUserCode = userInClaim.UserCode,
                                    ActionByRoleCode = userRoleInClaim.RoleCode,
                                    AssignedToRoleCode = request.AssignedToRoleCode,
                                    AssignedToUserCode = request.AssignedToUserCode,
                                    ActionDate = DateTime.Now.ToLocalTime(),
                                    Status = "forwarded",

                                };
                                await _unitofWork.TransportRouteHistory.AddAsync(routeApprovalHistory);

                            }
                        }
                        var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                                (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Created, null)));
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
        [HttpPost("updateTransportRouteDeatail")]
        public async Task<IActionResult> UpdateTranportRoute([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);

                if (decryptedData == null)
                {
                    var failData = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));

                    HttpContext.Response.Headers.Append("X-Data-Hash", failData.Hash);
                    return BadRequest(new { data = failData.EncryptedData });
                }

                TransportRouteVM request = JsonSerializer.Deserialize<TransportRouteVM>(decryptedData);

                if (!TryValidateModel(request))
                {
                    var badData = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest,
                            ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList())));

                    HttpContext.Response.Headers.Append("X-Data-Hash", badData.Hash);
                    return BadRequest(new { data = badData.EncryptedData });
                }
                var userRoleInClaim = await _unitofWork.UserRole
                    .FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));

                var userInClaim = await _unitofWork.User
                    .FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                var organizationclaimcode = await _unitofWork.Organization
                    .FirstOrDefaultAsync(x => x.OrganizationCode == User.FindFirst(SD.OrganizationCode).Value);

                var transporrouteindb = await _unitofWork.TransportRoute
                    .FirstOrDefaultAsync(x => x.TransportRouteCode == request.TransportRouteCode);

                if (transporrouteindb == null)
                {
                    var notFoundData = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));

                    HttpContext.Response.Headers.Append("X-Data-Hash", notFoundData.Hash);
                    return NotFound(new { data = notFoundData.EncryptedData });
                }
                await _unitofWork.TransportRoute.UpdateAsync(transporrouteindb.TransportRouteCode, async entity =>
                {
                    entity.DestinationAddress = request.DestinationAddress;
                    entity.DestinationContactNo = request.DestinationContactNo;
                    entity.VehicleCode = request.VehicleCode;
                    entity.Quantity = request.Quantity;
                    entity.TotalWeight = request.TotalWeight;
                    entity.DistanceInKm = request.DistanceInKm;
                    entity.PickupAddress = request.PickupAddress;
                    entity.ExpectedTravelTimeHours = request.ExpectedTravelTimeHours;
                    entity.ExpectedJourneyStart = request.ExpectedJourneyStart;
                    entity.Commodities = System.Text.Json.JsonSerializer.Serialize(request.Commodities);

                    entity.ExpectedJourneyEnd = request.ExpectedJourneyEnd;
                    entity.BaseAmount = request.BaseAmount;
                    entity.TollTax = request.TollTax;
                    entity.OtherCharges = request.OtherCharges;
                    entity.TotalCharge = request.TotalCharge;
                    entity.UpdatedDate = DateTime.Now.ToLocalTime();
                    entity.UpdatedBy = userInClaim.Name;
                    await Task.CompletedTask;
                });


                var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Updated, null)));

                HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                return Ok(new { data = okdata.EncryptedData });
            }
            catch (Exception Ex)
            {
                var errorData = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(
                    _encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));

                HttpContext.Response.Headers.Append("X-Data-Hash", errorData.Hash);
                return StatusCode(500, new { data = errorData.EncryptedData });
            }
        }
        [HttpPost("getAllTransportRouteByDistinationStatus")]
        public async Task<IActionResult> GetAllTransportRouteByDistinationStatus([FromBody] EncryptedDataVM Details)
        {
            try
            {

                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);
                if (decryptedData == null)
                {
                    var enc = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                        (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", enc.Hash);
                    return BadRequest(new { data = enc.EncryptedData });
                }

                StringValueVM request = JsonSerializer.Deserialize<StringValueVM>(decryptedData);

                if (!TryValidateModel(request))
                {
                    var enc = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                        (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", enc.Hash);
                    return BadRequest(new { data = enc.EncryptedData });
                }
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x =>
                    x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                var routeVMList = new List<GetTransportRouteVM>();
                List<TransportRoute> transportRoutes = new List<TransportRoute>();

                if (userInClaim.IsEntityUser)
                {
                    var organizationClaim = await _unitofWork.Organization.FirstOrDefaultAsync(x =>
                        x.OrganizationCode == User.FindFirst(SD.OrganizationCode).Value);

                    var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d =>
                        d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                    var userontranportdb = await _unitofWork.AdministorUser.FirstOrDefaultAsync(x => x.UserCode == userInClaim.UserCode);
                    if (userontranportdb !=null)
                    {
                        transportRoutes = (await _unitofWork.TransportRoute.GetAllAsync(x =>
                        x.Vehicle.OrganizationCode == organizationClaim.OrganizationCode &&
                            x.DestinationStatus == request.Value,
                            includeProperties: "Vehicle.VehicleType")).ToList();
                    }
                    else
                    {
                        transportRoutes = (await _unitofWork.TransportRoute
                            .GetAllAsync(x =>
                                x.UserCode == userInClaim.UserCode &&
                               x.DestinationStatus == request.Value,
                                includeProperties: "Vehicle.VehicleType")).ToList();
                    }
                }
                else
                {
                    transportRoutes = (await _unitofWork.TransportRoute.GetAllAsync(x =>
                       x.DestinationStatus == request.Value,
                        includeProperties: "Vehicle.VehicleType")).ToList();
                }

                foreach (var route in transportRoutes)
                {
                    var driverlist = await _unitofWork.VehicleDriverMap.GetAllAsync(x => x.VehicleCode == route.VehicleCode, includeProperties: "Vehicle,DriverDetail.User");

                    var gatepassindb = await _unitofWork.GatePass
                       .FirstOrDefaultAsync(h => h.TransportRouteCode == route.TransportRouteCode);
                    var historyList = await _unitofWork.TransportRouteHistory
                        .GetAllAsync(h => h.TransportRouteCode == route.TransportRouteCode);
                    var getSourceOrganization = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == route.SourceOrganizationCode);

                    var historyVM = new List<TransportRouteHistoryVM>();

                    routeVMList.Add(new GetTransportRouteVM
                    {
                        TransportRouteCode = route.TransportRouteCode,
                        SourceOrganization = route.SourceOrganizationCode,
                        DestinationAddress = route.DestinationAddress,
                        DestinationContactNo = route.DestinationContactNo,
                        Organization = getSourceOrganization??null,
                        VehicleCode = route.VehicleCode,
                        Vehicle = route.Vehicle,
                        Quantity = route.Quantity,
                        TotalWeight = route.TotalWeight,
                        DistanceInKm = route.DistanceInKm,
                        ExpectedTravelTimeHours = route.ExpectedTravelTimeHours,
                        ExpectedJourneyStart = route.ExpectedJourneyStart,
                        ExpectedJourneyEnd = route.ExpectedJourneyEnd,
                        ActualJourneyStart = route.ActualJourneyStart,
                        ActualJourneyEnd = route.ActualJourneyEnd,
                        BaseAmount = route.BaseAmount,
                        TollTax = route.TollTax,
                        OtherCharges = route.OtherCharges,
                        TotalCharge = route.TotalCharge,
                        ApprovalStatus = route.ApprovalStatus,
                        DestinationStatus = route.DestinationStatus,
                        IsCompleted = route.IsCompleted,
                        TransportRouteHistoryVM = historyVM,
                        PickupAddress = route.PickupAddress,
                        DriverList = driverlist?.ToList() ?? new List<VehicleDriverMap>(),

                        Commodities = route.Commodities == null
        ? null
        : JsonSerializer.Deserialize<List<Commodity>>(route.Commodities),

                        GatePass = gatepassindb ?? null,
                        CreatedBy = route.CreatedBy,
                        CreatedDate = route.CreatedDate,
                        UpdatedBy = route.UpdatedBy,
                        UpdatedDate = route.UpdatedDate,
                    });
                }

                var encryptedResponse = _encryptionHelper.Encrypt(
                    JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, routeVMList)));

                HttpContext.Response.Headers.Append("X-Data-Hash", encryptedResponse.Hash);
                return Ok(new { data = encryptedResponse.EncryptedData });
            }
            catch (Exception ex)
            {
                var errorResponse = _encryptionHelper.Encrypt(
                    JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, ex.Message)));

                HttpContext.Response.Headers.Append("X-Data-Hash", errorResponse.Hash);
                return StatusCode(500, new { data = errorResponse.EncryptedData });
            }

        }
        [HttpPost("getRoutebydriver")]
        public async Task<IActionResult> GetDriverRoute([FromBody] EncryptedDataVM Details)
        {

            try
            {

                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);
                if (decryptedData == null)
                {
                    var enc = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                        (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", enc.Hash);
                    return BadRequest(new { data = enc.EncryptedData });
                }

                StringValueVM request = JsonSerializer.Deserialize<StringValueVM>(decryptedData);

                if (!TryValidateModel(request))
                {
                    var enc = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                        (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", enc.Hash);
                    return BadRequest(new { data = enc.EncryptedData });
                }
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x =>
                    x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                var routeVMList = new List<GetTransportRouteVM>();
                List<TransportRoute> transportRoutes = new List<TransportRoute>();

                if (userInClaim.IsEntityUser)
                {
                    var organizationClaim = await _unitofWork.Organization.FirstOrDefaultAsync(x =>
                        x.OrganizationCode == User.FindFirst(SD.OrganizationCode).Value);

                    var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d =>
                        d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                    var driverdetailcode = await _unitofWork.DriverDetails.FirstOrDefaultAsync(x => x.UserCode == userInClaim.UserCode);
                    var driverindb = await _unitofWork.VehicleDriverMap.FirstOrDefaultAsync(x => x.DriverDetailCode == driverdetailcode.DriverDetailCode, includeProperties: "Vehicle");
                    var userontranportdb = await _unitofWork.AdministorUser.FirstOrDefaultAsync(x => x.UserCode == userInClaim.UserCode);


                    if (userontranportdb != null)
                    {
                        transportRoutes = (await _unitofWork.TransportRoute.GetAllAsync(x =>
                            x.VehicleCode == driverindb.VehicleCode &&
                            x.DestinationStatus == request.Value,
                            includeProperties: "Vehicle.VehicleType")).ToList();
                    }
                    else
                    {
                        transportRoutes = (await _unitofWork.TransportRoute
                            .GetAllAsync(x =>
                                   x.VehicleCode == driverindb.VehicleCode &&
                               x.DestinationStatus == request.Value,
                                includeProperties: "Vehicle.VehicleType")).ToList();
                    }
                }
                else
                {
                    transportRoutes = (await _unitofWork.TransportRoute.GetAllAsync(x =>
                       x.DestinationStatus == request.Value,
                        includeProperties: "Vehicle.VehicleType")).ToList();
                }

                foreach (var route in transportRoutes)
                {
                    var gatepassindb = await _unitofWork.GatePass
                       .FirstOrDefaultAsync(h => h.TransportRouteCode == route.TransportRouteCode);
                    var getSourceOrganization = await _unitofWork.Organization.FirstOrDefaultAsync(x => x.OrganizationCode == route.SourceOrganizationCode);


                    routeVMList.Add(new GetTransportRouteVM
                    {
                        TransportRouteCode = route.TransportRouteCode,
                        SourceOrganization = route.SourceOrganizationCode,
                        DestinationAddress = route.DestinationAddress,
                        DestinationContactNo = route.DestinationContactNo,
                        Organization= getSourceOrganization,
                        VehicleCode = route.VehicleCode,
                        Vehicle = route.Vehicle,
                        Quantity = route.Quantity,
                        TotalWeight = route.TotalWeight,
                        DistanceInKm = route.DistanceInKm,
                        ExpectedTravelTimeHours = route.ExpectedTravelTimeHours,
                        ExpectedJourneyStart = route.ExpectedJourneyStart,
                        ExpectedJourneyEnd = route.ExpectedJourneyEnd,
                        ActualJourneyStart = route.ActualJourneyStart,
                        ActualJourneyEnd = route.ActualJourneyEnd,
                        Commodities = route.Commodities == null
                        ? null
                        : JsonSerializer.Deserialize<List<Commodity>>(route.Commodities),
                        PickupAddress = route.PickupAddress,
                        BaseAmount = route.BaseAmount,
                        TollTax = route.TollTax,
                        OtherCharges = route.OtherCharges,
                        TotalCharge = route.TotalCharge,
                        ApprovalStatus = route.ApprovalStatus,
                        DestinationStatus = route.DestinationStatus,
                        IsCompleted = route.IsCompleted,
                        CreatedBy = route.CreatedBy,
                        CreatedDate = route.CreatedDate,
                        UpdatedBy = route.UpdatedBy,
                        UpdatedDate = route.UpdatedDate,
                        GatePass = gatepassindb ?? null,
                    });
                }

                var encryptedResponse = _encryptionHelper.Encrypt(
                    JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, routeVMList)));

                HttpContext.Response.Headers.Append("X-Data-Hash", encryptedResponse.Hash);
                return Ok(new { data = encryptedResponse.EncryptedData });
            }
            catch (Exception ex)
            {
                var errorResponse = _encryptionHelper.Encrypt(
                    JsonSerializer.Serialize<dynamic>(
                        _encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, ex.Message)));

                HttpContext.Response.Headers.Append("X-Data-Hash", errorResponse.Hash);
                return StatusCode(500, new { data = errorResponse.EncryptedData });
            }

        }
        [HttpPost("TransportRouteApproval")]
        public async Task<IActionResult> TransportRouteApproved([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);
                if (decryptedData != null)
                {
                    TransportRouteApprovalVM requestData = JsonSerializer.Deserialize<TransportRouteApprovalVM>(decryptedData);
                    if (TryValidateModel(requestData))
                    {
                        var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                        var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                        var transportrouteindb = await _unitofWork.TransportRouteHistory.FirstOrDefaultAsync(x => x.TransportRouteCode == requestData.TransportRouteCode);
                        if (transportrouteindb != null)
                        {
                            var getallroleindb = await _unitofWork.UserRole.GetAllAsync(x => x.RoleType == "department");
                            if ((userRoleInClaim.RoleLevel == RoleLevels.INTERMEDIATE && userRoleInClaim.RoleType == "department") ||
                                (userRoleInClaim.RoleLevel == RoleLevels.AUTHORITY && userRoleInClaim.RoleType == "department")
                                )
                            {
                                TransportRouteHistory routeApprovalHistory = new TransportRouteHistory()
                                {
                                    ApprovalCode = _unitofWork.TransportRoute.GenrateUniqueCode(),
                                    TransportRouteCode = requestData.TransportRouteCode,
                                    ActionByUserCode = userInClaim.UserCode,
                                    ActionByRoleCode = userRoleInClaim.RoleCode,
                                    AssignedToRoleCode = requestData.AssignedToRoleCode,
                                    AssignedToUserCode = requestData.AssignedToUserCode,
                                    ActionDate = DateTime.Now.ToLocalTime(),
                                    Status = "Approved",

                                };
                                await _unitofWork.TransportRouteHistory.AddAsync(routeApprovalHistory);
                                await _unitofWork.TransportRoute.UpdateAsync(transportrouteindb.TransportRouteCode, async entity =>
                                {
                                    entity.ApprovalStatus = "Approved";
                                    entity.DestinationStatus = "Created";
                                    await Task.CompletedTask;
                                });
                                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                               (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Updated, null)));
                                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                                return Ok(new { data = data.EncryptedData });
                            }
                            else
                            {

                                TransportRouteHistory routeApprovalHistory = new TransportRouteHistory()
                                {
                                    ApprovalCode = _unitofWork.TransportRoute.GenrateUniqueCode(),
                                    TransportRouteCode = requestData.TransportRouteCode,
                                    ActionByUserCode = userInClaim.UserCode,
                                    ActionByRoleCode = userRoleInClaim.RoleCode,
                                    AssignedToRoleCode = requestData.AssignedToRoleCode,
                                    AssignedToUserCode = requestData.AssignedToUserCode,
                                    ActionDate = DateTime.Now.ToLocalTime(),
                                    Status = "forwarded",

                                };
                                await _unitofWork.TransportRouteHistory.AddAsync(routeApprovalHistory);
                                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                            (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Updated, null)));
                                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                                return Ok(new { data = data.EncryptedData });
                            }
                        }
                        else
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                        (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return NotFound(new { data = data.EncryptedData });
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
        [HttpPost("rejectRoute")]
        public async Task<IActionResult> RejectRouteApprovel([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);
                if (decryptedData != null)
                {
                    StringValueVM requestData = JsonSerializer.Deserialize<StringValueVM>(decryptedData);
                    if (TryValidateModel(requestData))
                    {
                        var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                        var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                        var transportrouteindb = await _unitofWork.TransportRouteHistory.FirstOrDefaultAsync(x => x.TransportRouteCode == requestData.Value);
                        if (transportrouteindb != null)
                        {
                            var getallroleindb = await _unitofWork.UserRole.GetAllAsync(x => x.RoleType == "department");
                            if ((userRoleInClaim.RoleLevel == RoleLevels.INTERMEDIATE && userRoleInClaim.RoleType == "department") ||
                                (userRoleInClaim.RoleLevel == RoleLevels.AUTHORITY && userRoleInClaim.RoleType == "department")
                                )
                            {
                                TransportRouteHistory routeApprovalHistory = new TransportRouteHistory()
                                {
                                    ApprovalCode = _unitofWork.TransportRoute.GenrateUniqueCode(),
                                    TransportRouteCode = transportrouteindb.TransportRouteCode,
                                    ActionByUserCode = userInClaim.UserCode,
                                    ActionByRoleCode = userRoleInClaim.RoleCode,
                                    AssignedToRoleCode = null,
                                    AssignedToUserCode = null,
                                    ActionDate = DateTime.Now.ToLocalTime(),
                                    Status = "Rejected",

                                };
                                await _unitofWork.TransportRouteHistory.AddAsync(routeApprovalHistory);

                                await _unitofWork.TransportRoute.UpdateAsync(transportrouteindb.TransportRouteCode, async entity =>
                                {
                                    entity.ApprovalStatus = "Rejected";
                                    await Task.CompletedTask;
                                });
                                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                               (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Updated, null)));
                                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                                return Ok(new { data = data.EncryptedData });
                            }
                            else
                            {
                                var currentrolelevel = userRoleInClaim.RoleLevel;
                                var nextRole = getallroleindb.Where(r => r.RoleLevel > currentrolelevel && r.RoleType == "department").OrderBy(r => r.RoleLevel).FirstOrDefault();
                                await _unitofWork.TransportRouteHistory.UpdateAsync(transportrouteindb.ApprovalCode, async entity =>
                                {
                                    entity.ActionByUserCode = userInClaim.UserCode;
                                    entity.ActionByRoleCode = userRoleInClaim.RoleCode;
                                    entity.AssignedToRoleCode = nextRole.RoleCode;
                                    entity.ActionDate = DateTime.Now.ToLocalTime();
                                    await Task.CompletedTask;
                                });
                                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                            (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.Updated, null)));
                                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                                return Ok(new { data = data.EncryptedData });
                            }
                        }
                        else
                        {
                            var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                        (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                            return NotFound(new { data = data.EncryptedData });
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

        [HttpPost("GenerateGatePass")]
        public async Task<IActionResult> CreateGatePass([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);
                if (decryptedData == null)
                {
                    var enc = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                        (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", enc.Hash);
                    return BadRequest(new { data = enc.EncryptedData });
                }

                StringValueVM request = JsonSerializer.Deserialize<StringValueVM>(decryptedData);


                if (!TryValidateModel(request))
                {
                    var enc = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                        (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", enc.Hash);
                    return BadRequest(new { data = enc.EncryptedData });
                }
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                var transportapproved = await _unitofWork.TransportRoute.FirstOrDefaultAsync(x => x.TransportRouteCode == request.Value);
                if (transportapproved.ApprovalStatus == "Approved")
                {
                    var gatePass = new GatePass
                    {
                        GatePassCode = _unitofWork.GatePass.GenrateUniqueCode(),
                        TransportRouteCode = request.Value,
                        DepartureTime = DateTime.Now.ToLocalTime(),
                        IssuedByUserCode = userInClaim.Name + "/" + userInClaim.UserCode,
                        IssueDate = DateTime.Now.ToLocalTime(),
                        QRCodePath = null,
                        GatePassFilePath = null,
                    };

                    await _unitofWork.GatePass.AddAsync(gatePass);
                    await _unitofWork.TransportRoute.UpdateAsync(request.Value, async entity =>
                    {
                        entity.DestinationStatus = "Dispatched";
                        await Task.CompletedTask;

                    });

                    var success = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                        (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, gatePass)));

                    HttpContext.Response.Headers.Append("X-Data-Hash", success.Hash);
                    return Ok(new { data = success.EncryptedData });
                }
                else
                {
                    var enc = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                  (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.BadRequest, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", enc.Hash);
                    return BadRequest(new { data = enc.EncryptedData });
                }

            }
            catch (Exception ex)
            {
                var err = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                    (_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, ex.Message)));

                HttpContext.Response.Headers.Append("X-Data-Hash", err.Hash);
                return StatusCode(500, new { data = err.EncryptedData });
            }
        }
        [HttpPost("GetGatePassDetail")]
        public async Task<IActionResult> GetGatePassDetail([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);
                if (decryptedData == null)
                {
                    var enc = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                        (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", enc.Hash);
                    return BadRequest(new { data = enc.EncryptedData });
                }

                StringValueVM request = JsonSerializer.Deserialize<StringValueVM>(decryptedData);

                if (!TryValidateModel(request))
                {
                    var enc = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                        (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", enc.Hash);
                    return BadRequest(new { data = enc.EncryptedData });
                }
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                var gatepassindb = await _unitofWork.GatePass.FirstOrDefaultAsync(x => x.TransportRouteCode == request.Value, includeProperties: "Vehicle,TransportRoute");
                if (gatepassindb == null)
                {

                    var enc = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                        (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", enc.Hash);
                    return NotFound(new { data = enc.EncryptedData });
                }
                var success = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                    (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, gatepassindb)));

                HttpContext.Response.Headers.Append("X-Data-Hash", success.Hash);
                return Ok(new { data = success.EncryptedData });
            }
            catch (Exception ex)
            {
                var err = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                    (_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, ex.Message)));

                HttpContext.Response.Headers.Append("X-Data-Hash", err.Hash);
                return StatusCode(500, new { data = err.EncryptedData });
            }
        }
        [HttpPost("startRoute")]
        public async Task<IActionResult> StartRoute([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);
                if (decryptedData == null)
                {
                    var enc = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                        (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", enc.Hash);
                    return BadRequest(new { data = enc.EncryptedData });
                }

                StringValueVM request = JsonSerializer.Deserialize<StringValueVM>(decryptedData);

                if (!TryValidateModel(request))
                {
                    var enc = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                        (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", enc.Hash);
                    return BadRequest(new { data = enc.EncryptedData });
                }
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                var gettransportindb = await _unitofWork.TransportRoute.FirstOrDefaultAsync(x => x.TransportRouteCode == request.Value);
                await _unitofWork.TransportRoute.UpdateAsync(gettransportindb.TransportRouteCode, async entity =>
                {
                    entity.ActualJourneyStart = DateTime.Now.ToLocalTime();
                    entity.DestinationStatus = "Dispatched";
                    await Task.CompletedTask;

                });
                var success = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic> (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Updated, gettransportindb)));

                HttpContext.Response.Headers.Append("X-Data-Hash", success.Hash);
                return Ok(new { data = success.EncryptedData });
            }
            catch (Exception ex)
            {
                var err = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                    (_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, ex.Message)));

                HttpContext.Response.Headers.Append("X-Data-Hash", err.Hash);
                return StatusCode(500, new { data = err.EncryptedData });
            }
        }
        [HttpPost("completeRoute")]
        public async Task<IActionResult> CompleteRoute([FromBody] EncryptedDataVM Details)
        {
            try
            {
                var decryptedData = _encryptionHelper.ValidateDataHashAndData(HttpContext.Request.Headers, Details.Data);
                if (decryptedData == null)
                {
                    var enc = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                        (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", enc.Hash);
                    return BadRequest(new { data = enc.EncryptedData });
                }

                StringValueVM request = JsonSerializer.Deserialize<StringValueVM>(decryptedData);

                if (!TryValidateModel(request))
                {
                    var enc = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                        (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.UnSuccessful, null)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", enc.Hash);
                    return BadRequest(new { data = enc.EncryptedData });
                }
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));

                var gettransportindb = await _unitofWork.TransportRoute.FirstOrDefaultAsync(x => x.TransportRouteCode == request.Value);
                await _unitofWork.TransportRoute.UpdateAsync(gettransportindb.TransportRouteCode, async entity =>
                {
                    entity.ActualJourneyEnd = DateTime.Now.ToLocalTime();
                    entity.DestinationStatus = "Delivered";
                    entity.IsCompleted = true;
                    await Task.CompletedTask;

                });
                var success = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                    (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Updated, gettransportindb)));

                HttpContext.Response.Headers.Append("X-Data-Hash", success.Hash);
                return Ok(new { data = success.EncryptedData });
            }
            catch (Exception ex)
            {
                var err = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                    (_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, ex.Message)));

                HttpContext.Response.Headers.Append("X-Data-Hash", err.Hash);
                return StatusCode(500, new { data = err.EncryptedData });
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
                        var vehicletypeindb = await _unitofWork.TransportRoute.FirstOrDefaultAsync(x => x.TransportRouteCode == user.Value && x.ApprovalStatus != "Approved");
                        if (vehicletypeindb == null)
                        {
                            var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                           (_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, null)));
                            HttpContext.Response.Headers.Append("X-Data-Hash", okdata.Hash);
                            return NotFound(new { data = okdata.EncryptedData });
                        }
                        else
                        {
                            var alltranporthistory = await _unitofWork.TransportRouteHistory.GetAllAsync(x => x.TransportRouteCode == user.Value);
                            foreach (var item in alltranporthistory)
                            {
                                await _unitofWork.TransportRouteHistory.RemoveAsync(item.ApprovalCode);

                            }

                            await _unitofWork.TransportRoute.RemoveAsync(vehicletypeindb.TransportRouteCode);
                            var okdata = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>
                           (_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Deleted, null)));
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

    }
}
