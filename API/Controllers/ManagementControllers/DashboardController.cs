using API.Data.IRepositories;
using API.Helpers;
using API.Helpers.Models;
using API.Model.QueryParamViewModels;
using API.Model.UserModels;
using API.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Utilities.Collections;
using System.Security.Claims;
using System.Text.Json;

namespace API.Controllers.ManagementControllers
{
    [ApiController]
    [Route(SD.baseUrl + "dashboard")]
    [Authorize(Policy = SD.IsAccess)]
    public class DashboardController : ControllerBase
    {
        private readonly IUnitofWork _unitofWork;
        private readonly IEncryptionHelper _encryptionHelper;
        public DashboardController(IUnitofWork unitofWork, IEncryptionHelper encryptionHelper)
        {

            _unitofWork = unitofWork;
            _encryptionHelper = encryptionHelper;
        }
        [HttpGet("bankadminDashboard")]
        public async Task<IActionResult> BankAdminDashboard()
        {
            try
            {
                var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                var getadmionstor = await _unitofWork.AdministorUser.FirstOrDefaultAsync(x => x.UserCode == userInClaim.UserCode);
                if (getadmionstor == null)
                {
                    var notLoginData = _encryptionHelper.Encrypt(JsonSerializer.Serialize(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, "Admin not login")));
                    HttpContext.Response.Headers.Append("X-Data-Hash", notLoginData.Hash);
                    return NotFound(new { data = notLoginData.EncryptedData });
                }
                else
                {
                    var orgCode = getadmionstor.OrganizationCode;
                    var currentMonth = DateTime.Now.Month;
                    var currentYear = DateTime.Now.Year;
                    var allBranches = (await _unitofWork.BankBranch.GetAllAsync(x => x.OrganizationCode == orgCode)).ToList();
                    var allBankUsers = (await _unitofWork.BankUser.GetAllAsync(x => x.BankBranch.OrganizationCode == orgCode)).ToList();
                    var allPaymentResponseFiles = (await _unitofWork.UploadFileInfo.GetAllAsync(x => x.ResponseOrganizationCode == orgCode)).ToList();
                    var pendingPaymentFiles = allPaymentResponseFiles.Where(x => x.Status == "InProcess").ToList();
                    var thisMonthResponses = allPaymentResponseFiles.Where(x => x.ResponseDate.Month == currentMonth && x.ResponseDate.Year == currentYear).ToList();
                    var dashboardData = new
                    {
                        TotalBranches = allBranches.Count,
                        TotalBankUsers = allBankUsers.Count,
                        TotalPaymentResponseFiles = allPaymentResponseFiles.Count,
                        PendingPaymentFiles = pendingPaymentFiles.Count,
                        ThisMonthResponseFilesCount = thisMonthResponses.Count,
                        ThisMonthResponses = thisMonthResponses
                    };

                    var encrypted = _encryptionHelper.Encrypt(JsonSerializer.Serialize(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, dashboardData)));

                    HttpContext.Response.Headers.Append("X-Data-Hash", encrypted.Hash);
                    return Ok(new { data = encrypted.EncryptedData });
                }

            }
            catch (Exception ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize(_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new { data = data.EncryptedData });
            }
        }
        [HttpGet("transportadminDashboard")]
        public async Task<IActionResult> TransportAdminDashboard()
        {
            try
            {
                var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                var getadmionstor = await _unitofWork.AdministorUser.FirstOrDefaultAsync(x => x.UserCode == userInClaim.UserCode);
                if (getadmionstor != null)
                {
                    var currentMonth = DateTime.Now.Month;
                    var currentYear = DateTime.Now.Year;
                    var allroute = (await _unitofWork.TransportRoute.GetAllAsync(x => x.Vehicle.OrganizationCode == getadmionstor.OrganizationCode && x.DestinationStatus != null)).ToList();
                    var allvehicle = (await _unitofWork.Vehicle.GetAllAsync(x => x.OrganizationCode == getadmionstor.OrganizationCode)).ToList();
                    var delivedroutes = (await _unitofWork.TransportRoute.GetAllAsync(x => x.Vehicle.OrganizationCode == getadmionstor.OrganizationCode && x.DestinationStatus == "Delivered")).ToList();
                    var dispatchedroute = (await _unitofWork.TransportRoute.GetAllAsync(x => x.Vehicle.OrganizationCode == getadmionstor.OrganizationCode && x.DestinationStatus == "Dispatched")).ToList();
                    var createdroute = (await _unitofWork.TransportRoute.GetAllAsync(x => x.Vehicle.OrganizationCode == getadmionstor.OrganizationCode && x.DestinationStatus == "Created")).ToList();
                    var thismonthroutes = (await _unitofWork.TransportRoute.GetAllAsync(x => x.Vehicle.OrganizationCode == getadmionstor.OrganizationCode && x.CreatedDate.Month == currentMonth && x.CreatedDate.Year == currentYear && x.DestinationStatus != null)).ToList();
                    var allUsers = (await _unitofWork.TransportUser.GetAllAsync(x => x.OrganizationCode == getadmionstor.OrganizationCode)).ToList();
                    var dashboardData = new
                    {
                        TotalRoutes = allroute.Count,
                        TotalUsers = allUsers.Count,
                        ThisMonthRoutesCount = thismonthroutes.Count,
                        ThisMonthRoutes = thismonthroutes,
                        CreateRoute = createdroute.Count,
                        DispatchedRoute = dispatchedroute.Count,
                        DeliveredRoute = delivedroutes.Count,
                        VehicleCount = allvehicle.Count,
                    };
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, dashboardData)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return Ok(new { data = data.EncryptedData });

                }
                else
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, "Admin  not login")));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return NotFound(new { data = data.EncryptedData });
                }

            }
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new
                {
                    data = data.EncryptedData
                });
            }
        }
        [HttpGet("transportdriverDashboard")]
        public async Task<IActionResult> TransportDriverDashboard()
        {
            try
            {
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                var driverdetailcode = await _unitofWork.DriverDetails.FirstOrDefaultAsync(x => x.UserCode == userInClaim.UserCode);
                var driverindb = await _unitofWork.VehicleDriverMap.FirstOrDefaultAsync(x => x.DriverDetailCode == driverdetailcode.DriverDetailCode, includeProperties: "Vehicle");
                if (driverindb != null)
                {
                    var currentMonth = DateTime.Now.Month;
                    var currentYear = DateTime.Now.Year;
                    var delivedroutes = (await _unitofWork.TransportRoute.GetAllAsync(x => x.VehicleCode == driverindb.VehicleCode && x.DestinationStatus == "Delivered")).ToList();
                    var dispatchedroute = (await _unitofWork.TransportRoute.GetAllAsync(x => x.VehicleCode == driverindb.VehicleCode && x.DestinationStatus == "Dispatched")).ToList();
                    var thismonthroutes = (await _unitofWork.TransportRoute.GetAllAsync(x => x.VehicleCode == driverindb.VehicleCode && x.CreatedDate.Month == currentMonth && x.CreatedDate.Year == currentYear && x.DestinationStatus != "Created" && x.DestinationStatus != null)).ToList();
                    var dashboardData = new
                    {
                        ThisMonthRoutesCount = thismonthroutes.Count,
                        ThisMonthRoutes = thismonthroutes,
                        DispatchedRoute = dispatchedroute.Count,
                        DeliveredRoute = delivedroutes.Count,
                    };
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, dashboardData)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return Ok(new { data = data.EncryptedData });



                }
                else
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, "No Vehicle Assigned")));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return NotFound(new { data = data.EncryptedData });
                }

            }
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new
                {
                    data = data.EncryptedData
                });
            }
        }
        [HttpGet("departmentadminDashboard")]
        public async Task<IActionResult> OrganizationAdminDashboard()
        {
            try
            {
                var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                var getadmionstor = await _unitofWork.AdministorUser.FirstOrDefaultAsync(x => x.UserCode == userInClaim.UserCode);
                if (getadmionstor != null)
                {
                    var currentMonth = DateTime.Now.Month;
                    var currentYear = DateTime.Now.Year;
                    var allroute = (await _unitofWork.TransportRoute.GetAllAsync(x => x.SourceOrganizationCode == getadmionstor.OrganizationCode)).ToList();
                    var Pendingroute = (await _unitofWork.TransportRoute.GetAllAsync(x => x.SourceOrganizationCode == getadmionstor.OrganizationCode && x.ApprovalStatus == "Pending")).ToList();
                    var thismonthroutes = (await _unitofWork.TransportRoute.GetAllAsync(x => x.SourceOrganizationCode == getadmionstor.OrganizationCode && x.CreatedDate.Month == currentMonth && x.CreatedDate.Year == currentYear)).ToList();
                    var thismonthfile = (await _unitofWork.UploadFileInfo.GetAllAsync(x => x.OrganizationCode == getadmionstor.OrganizationCode && x.RequestedDate.Month == currentMonth && x.RequestedDate.Year == currentYear)).ToList();
                    var allOffices = (await _unitofWork.Office.GetAllAsync(x => x.OrganizationCode == getadmionstor.OrganizationCode)).ToList();
                    var allUsers = (await _unitofWork.DepartmentUser.GetAllAsync(x => x.Office.OrganizationCode == getadmionstor.OrganizationCode)).ToList();
                    var allPaymentFiles = (await _unitofWork.UploadFileInfo.GetAllAsync(x => x.OrganizationCode == getadmionstor.OrganizationCode)).ToList();
                    var dashboardData = new
                    {
                        TotalRoutes = allroute.Count,
                        PendingRoute = Pendingroute.Count,
                        TotalOffices = allOffices.Count,
                        TotalUsers = allUsers.Count,
                        TotalPaymentFiles = allPaymentFiles.Count,
                        ThisMonthRoutesCount = thismonthroutes.Count,
                        ThisMonthRoutes = thismonthroutes
                    };
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, dashboardData)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return Ok(new { data = data.EncryptedData });

                }
                else
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, "Admin  not login")));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return NotFound(new { data = data.EncryptedData });
                }

            }
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new
                {
                    data = data.EncryptedData
                });
            }
        }
        [HttpGet("departmentUserDashboard")]
        public async Task<IActionResult> DepartmentUserDashboard()
        {
            try
            {
                var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                var departmentUser = await _unitofWork.DepartmentUser.FirstOrDefaultAsync(x => x.UserCode == userInClaim.UserCode, includeProperties: "Office.Organization");
                if (departmentUser != null)
                {
                    var currentMonth = DateTime.Now.Month;
                    var currentYear = DateTime.Now.Year;
                    //pending
                    var latestHistories = (await _unitofWork.TransportRouteHistory.GetAllAsync()).GroupBy(x => x.TransportRouteCode).Select(g => g.OrderByDescending(x => x.Id).First()).ToList();
                    var history = latestHistories.Where(h => h.AssignedToUserCode == userInClaim.UserCode && h.AssignedToRoleCode == userRoleInClaim.RoleCode).ToList();
                    var codes = history.Select(h => h.TransportRouteCode).Distinct().ToList();
                    var pendingtransportRoutes = (await _unitofWork.TransportRoute.GetAllAsync(x => codes.Contains(x.TransportRouteCode) && x.SourceOrganizationCode == departmentUser.Office.OrganizationCode, includeProperties: "Vehicle.VehicleType,Vehicle.Organization")).ToList();
                    //dashboarddata
                    var dashboardHistories = (await _unitofWork.TransportRouteHistory.GetAllAsync(h => (h.AssignedToUserCode == userInClaim.UserCode && h.AssignedToRoleCode == userRoleInClaim.RoleCode) || (h.ActionByUserCode == userInClaim.UserCode && h.ActionByRoleCode == userRoleInClaim.RoleCode))).GroupBy(x => x.TransportRouteCode).Select(g => g.Key).ToList();
                    var transportRoutes = (await _unitofWork.TransportRoute.GetAllAsync(x => dashboardHistories.Contains(x.TransportRouteCode) && x.SourceOrganizationCode == departmentUser.Office.OrganizationCode, includeProperties: "Vehicle.VehicleType,Vehicle.Organization")).ToList();
                    var thisMonthRoutes = transportRoutes.Where(x => x.CreatedDate.Month == currentMonth && x.CreatedDate.Year == currentYear).ToList();
                    var allfile = (await _unitofWork.UploadFileInfoHistory.GetAllAsync(h => h.ActionByUserCode == userInClaim.UserCode && h.ActionByRoleCode == userRoleInClaim.RoleCode || h.AssignedToUserCode == userInClaim.UserCode && h.AssignedToRoleCode == userRoleInClaim.RoleCode)).GroupBy(x => x.FileInfoCode).ToList();
                    var fileCodesboth = allfile.Select(h => h.Key).Distinct().ToList();
                    var totalfiles = (await _unitofWork.UploadFileInfo.GetAllAsync(x => fileCodesboth.Contains(x.FileInfoCode))).ToList();
                    var latestFileHistoriesforpending = (await _unitofWork.UploadFileInfoHistory.GetAllAsync()).GroupBy(x => x.FileInfoCode).Select(g => g.OrderByDescending(x => x.Id).First()).ToList();
                    var pendinghistory = latestFileHistoriesforpending.Where(h => h.AssignedToUserCode == userInClaim.UserCode && h.AssignedToRoleCode == userRoleInClaim.RoleCode).ToList();

                    var fileCodes = pendinghistory.Select(h => h.FileInfoCode).Distinct().ToList();
                    var uploadFiles = (await _unitofWork.UploadFileInfo.GetAllAsync(x => fileCodes.Contains(x.FileInfoCode))).ToList();
                    var pendingFilesforuser = uploadFiles.Where(x => x.DepartmentApprovalStatus == "Pending").ToList();
                    var ApprovedFiles = uploadFiles.Where(x => x.DepartmentApprovalStatus == "Approved").ToList();
                    var RejectedFiles = uploadFiles.Where(x => x.DepartmentApprovalStatus == "Rejected").ToList();
                    var createdroutes = transportRoutes.Where(x => x.DestinationStatus == "Created").ToList();
                    var dileverdroutes = transportRoutes.Where(x => x.DestinationStatus == "Delivered").ToList();
                    var dispatchroutes = transportRoutes.Where(x => x.DestinationStatus == "Dispatched").ToList();
                    var thisMonthFiles = uploadFiles.Where(x => x.RequestedDate.Month == currentMonth && x.RequestedDate.Year == currentYear).ToList();
                    var dashboardData = new
                    {
                        TotalRoutes = transportRoutes.Count,
                        PendingRoutes = pendingtransportRoutes.Count,
                        CreatedRoutes = createdroutes.Count,
                        DispatchedRoutes = dispatchroutes.Count,
                        DeliveredRoutes = dileverdroutes.Count,
                        ThisMonthRoutesCount = thisMonthRoutes.Count,
                        TotalFiles = totalfiles.Count,
                        PendingFiles = pendingFilesforuser.Count,
                        ApprovedFiles = ApprovedFiles.Count,
                        RejectedFiles = RejectedFiles.Count,
                        ThisMonthFilesCount = thisMonthFiles.Count,
                        ThisMonthRoutes = thisMonthRoutes,
                        ThisMonthFiles = thisMonthFiles
                    };
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, dashboardData)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return Ok(new { data = data.EncryptedData });

                }
                else
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, "user  not fund")));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return NotFound(new { data = data.EncryptedData });
                }

            }
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new
                {
                    data = data.EncryptedData
                });
            }
        }

        [HttpGet("bankUserDashboard")]
        public async Task<IActionResult> BankUserDashboard()
        {
            try
            {
                var userRoleInClaim = await _unitofWork.UserRole.FirstOrDefaultAsync(d => d.RoleCode == User.FindFirstValue(ClaimTypes.Role));
                var userInClaim = await _unitofWork.User.FirstOrDefaultAsync(x => x.UserCode == User.FindFirstValue(ClaimTypes.SerialNumber));
                var departmentUser = await _unitofWork.BankUser.FirstOrDefaultAsync(x => x.UserCode == userInClaim.UserCode, includeProperties: "BankBranch.Organization");
                if (departmentUser != null)
                {
                    var currentMonth = DateTime.Now.Month;
                    var currentYear = DateTime.Now.Year;
                    var uploadFiles = (await _unitofWork.UploadFileInfo.GetAllAsync(x => x.BankBranchCode == departmentUser.BankBranchCode && x.ResponseUserCode==userInClaim.UserCode)).ToList();
                    var pendingFiles = uploadFiles.Where(x => x.Status == "InProcess").ToList();
                    var ApprovedFiles = uploadFiles.Where(x => x.Status == "Response" && x.ResponseUserCode == userInClaim.UserCode).ToList();
                    var thisMonthResponsePaymentFiles = uploadFiles.Where(x => x.ResponseDate.Month == currentMonth && x.ResponseDate.Year == currentYear && x.ResponseUserCode == userInClaim.UserCode).ToList();
                    var thisMonthPendingPaymentFiles = uploadFiles.Where(x => x.InprocessDate.Month == currentMonth && x.InprocessDate.Year == currentYear).ToList();
                    var dashboardData = new
                    {
                        TotalFiles = uploadFiles.Count,
                        PendingFiles = pendingFiles.Count,
                        ApprovedFiles = ApprovedFiles.Count,
                        ThisMonthFilesPendingCount = thisMonthPendingPaymentFiles.Count,
                        ThisMonthPendingFiles = thisMonthPendingPaymentFiles,
                        ThisMonthResponsefileCount = thisMonthResponsePaymentFiles.Count,
                        ThisMonthResponseFiles = thisMonthResponsePaymentFiles
                    };
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(true, StatusType.success, ResponseHandler.Ok, dashboardData)));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return Ok(new { data = data.EncryptedData });

                }
                else
                {
                    var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.failure, ResponseHandler.NotFound, "user  not fund")));
                    HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                    return NotFound(new { data = data.EncryptedData });
                }

            }
            catch (Exception Ex)
            {
                var data = _encryptionHelper.Encrypt(JsonSerializer.Serialize<dynamic>(_encryptionHelper.GenrateResponse(false, StatusType.error, ResponseHandler.Default, Ex.Message)));
                HttpContext.Response.Headers.Append("X-Data-Hash", data.Hash);
                return StatusCode(500, new
                {
                    data = data.EncryptedData
                });
            }
        }

    }
}
