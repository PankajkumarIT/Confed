
using API.Data.IRepositories.IManagmentRepositories;
using API.Data.IRepositories.IManagmentRepositories.IBankManagementRepositories;
using API.Data.IRepositories.IManagmentRepositories.IDepartmentManagementRepositories;
using API.Data.IRepositories.IManagmentRepositories.ITransportMangementRepositories;
using API.Data.IRepositories.IManagmentRepositories.IUserManagementRepositories;
using API.Data.IRepository.AreaRepositories;
using API.Data.IRepository.UserRepositories;
using System.Diagnostics;
namespace API.Data.IRepositories
{
    public interface IUnitofWork
    {
        IStateRepository State { get; }
        IDistrictRepository District { get; }
        ICtvRepository Ctv { get; }
        IUserRoleRepository UserRole { get; }

        IRoleAccessRepository RoleAccess { get; }
        IUserRepository User { get; }
        IUserLoginRepository UserLogin { get; }
        IMenuRepository Menu { get; }
        IMenuAccessRepository MenuAccess { get; }
        IRouteRepository Route { get; }
        IRouteAccessRepository RouteAccess { get; }
        IOrganizationRepository Organization { get; }
        IVehicleRepository Vehicle { get; }
        IVehicleTypeRepository VehicleType { get; }
        ITransportUserRepository TransportUser { get; }
        IDriverDetailsRepository DriverDetails { get; }
        IVehicleDriverMapRepository VehicleDriverMap { get; }
        IDepartmentUserRepository DepartmentUser { get; }
        ITransportRouteRepository TransportRoute { get; }
        IUnitMasterRepository UnitMaster { get; }
        ITransportRouteHistoryRepository TransportRouteHistory { get; }
        IGatePassRepository GatePass { get; }
        IOfficeRepository Office { get; }
        IBankBranchRepository BankBranch { get; }
        IDesignationRepository Designation { get; }
        IBankUserRepository BankUser { get; }
        IAdministorUserRepository AdministorUser { get; }
        IUploadFileInfoRepository UploadFileInfo { get; }
        IUploadFileInfoHistoryRepository UploadFileInfoHistory { get; }
        IDepartmentBankOrganizationMapRepository DepartmentBankOrganizationsMap { get; }
        ICommodityMasterRepository CommodityMaster { get; }
        IUserFileDirectoryRepository UserFileDirectory { get; }
    }
}
