
using API.Data.IRepositories;
using API.Data.IRepositories.IManagmentRepositories;
using API.Data.IRepositories.IManagmentRepositories.IBankManagementRepositories;
using API.Data.IRepositories.IManagmentRepositories.IDepartmentManagementRepositories;
using API.Data.IRepositories.IManagmentRepositories.ITransportMangementRepositories;
using API.Data.IRepositories.IManagmentRepositories.IUserManagementRepositories;
using API.Data.IRepository.AreaRepositories;
using API.Data.IRepository.UserRepositories;
using API.Data.Repositories.ManagmentRepositories;
using API.Data.Repositories.ManagmentRepositories.BankManagementRepositories;
using API.Data.Repositories.ManagmentRepositories.DepartmentManagementRepositoreis;
using API.Data.Repositories.ManagmentRepositories.TransportMangementRepositories;
using API.Data.Repositories.ManagmentRepositories.UserManagementRepositories;
using API.Data.Repositories.UserRepositories;
using API.Data.Repository.AreaRepositories;
using API.Data.Repository.UserRepositories;
using API.Helpers.Models;
using API.Model.ManagementModels.DepartmentManagement;
using Microsoft.Extensions.Options;


namespace API.Data.Repositories
{
    public class UnitOfWork : IUnitofWork
    {
        private readonly ApplicationDbContext _context;
        private HttpClient _httpClient;
        private readonly IOptions<AppSettings> _appsettings;
        private readonly IOptions<S3ServiceModel> _s3ServiceModel;

        public UnitOfWork(ApplicationDbContext context, HttpClient httpClient, IOptions<AppSettings> appSettings, IOptions<S3ServiceModel> options)
        {

            _context = context;
            _httpClient = httpClient;
            _appsettings = appSettings;
            _s3ServiceModel = options;
            UserRole = new UserRoleRepository(_context);
            RoleAccess = new RoleAccessRepository(_context);
            UserLogin = new UserLoginRepository(_context);
            MenuAccess = new MenuAccessRepository(_context);
            Menu = new MenuRepository(_context);
            Route = new RouteRepository(_context);
            RouteAccess = new RouteAccessRepository(_context);
            State = new StateRepository(_context);
            District = new DistrictRepository(_context);
            Ctv = new CtvRepository(_context);
            TransportRoute = new TransportRouteRepository(_context);
            UnitMaster = new UnitMasterRepository(_context);
            TransportRouteHistory = new TransportRouteHistoryRepository(_context);
            GatePass = new GatePassRepository (_context);
            User = new UserRepository(_context, _appsettings);
            BankBranch = new BankBranchRepository(_context);
            BankUser = new BankUserRepository(_context);
            Office = new OfficeRepository(_context);
            DepartmentUser = new DepartmentUserRepository(_context);
            Designation = new DesignationRepository(_context);
            Organization = new OrganizationRepository(_context);
            AdministorUser = new AdministorUserRepository(_context);
            DriverDetails = new DriverDetailsRepository(_context);
            VehicleDriverMap = new VehicleDriverMapRepository(_context);
            TransportUser = new TransportUserRepository(_context);
            VehicleType = new VehicleTypeRepository(_context);
            Vehicle = new VehicleRepository(_context);
            UploadFileInfo = new UploadFileInfoRepository(_context, _s3ServiceModel);
            UploadFileInfoHistory = new UploadFileInfoHistoryRepository(_context);
            DepartmentBankOrganizationsMap = new DepartmentBankOrganizationMapRepository(_context);
            CommodityMaster = new CommodityMasterRepository(_context);
            UserFileDirectory = new UserFileDirectoryRepository(_context);
        }
            
        public IUserRepository User { private set; get; }
        public IUserRoleRepository UserRole { private set; get; }
        public IRoleAccessRepository RoleAccess { private set; get; }
        public IUserLoginRepository UserLogin { private set; get; }
        public IMenuRepository Menu { private set; get; }
        public IMenuAccessRepository MenuAccess { private set; get; }
        public IRouteRepository Route { private set; get; }
        public IRouteAccessRepository RouteAccess { private set; get; }
        public IOrganizationRepository Organization { private set; get; }
        public IVehicleRepository Vehicle { private set; get; }
        public IVehicleTypeRepository VehicleType { private set; get; }
        public ITransportUserRepository TransportUser { private set; get; }
        public IDriverDetailsRepository DriverDetails { private set; get; }
        public IVehicleDriverMapRepository VehicleDriverMap { private set; get; }
        public IDepartmentUserRepository DepartmentUser { private set; get; }
        public IStateRepository State { private set; get; }
        public IDistrictRepository District { private set; get; }
        public ICtvRepository Ctv { private set; get; }
        public ITransportRouteRepository TransportRoute { private set; get; }
        public IUnitMasterRepository UnitMaster { private set; get; }
        public ITransportRouteHistoryRepository TransportRouteHistory { private set; get; }
        public IGatePassRepository GatePass { private set; get; }
        public IOfficeRepository Office { private set; get; }
        public IDesignationRepository Designation { private set; get; }
        public IBankUserRepository BankUser { private set; get; }
        public IAdministorUserRepository AdministorUser { private set; get; }
        public IBankBranchRepository BankBranch { private set; get; }

        public IUploadFileInfoRepository UploadFileInfo { private set; get; }

        public IUploadFileInfoHistoryRepository UploadFileInfoHistory { private set; get; }

        public IDepartmentBankOrganizationMapRepository DepartmentBankOrganizationsMap { private set; get; }

        public ICommodityMasterRepository CommodityMaster { private set; get; }

        public IUserFileDirectoryRepository UserFileDirectory { private set; get; }
    }
}