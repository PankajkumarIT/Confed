
using API.Model.AreaModels;
using API.Model.ExceptionHandlerModels;
using API.Model.ManagementModels;
using API.Model.ManagementModels.BankManagement;
using API.Model.ManagementModels.DepartmentManagement;
using API.Model.ManagementModels.TransporterManagement;
using API.Model.ManagementModels.UserModels;
using API.Model.Menus;
using API.Model.Routes;
using API.Model.UserModels;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class ApplicationDbContext : DbContext

    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {


        }

        public DbSet<UserRole> UserRole { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<RoleAccess> RoleAccess { get; set; }

        public DbSet<UserLogin> UserLogin { get; set; }

        public DbSet<Menu> Menu { get; set; }
        public DbSet<MenuAccess> MenuAccess { get; set; }

        private DbSet<Model.Routes.Route> route;

        public DbSet<RouteAccess> RouteAccess { get; set; }
        public DbSet<Vehicle> Vehicle { get; set; }
        public DbSet<DriverDetails> DriverDetails { get; set; }
        public DbSet<VehicleType> VehicleType { get; set; }
        public DbSet<Organization> Organization { get; set; }
        public DbSet<TransporterUser> TransporterUser { get; set; }
        public DbSet<VehicleDriverMap> VehicleDriverMap { get; set; }
        public DbSet<UnitMaster> UnitMaster { get; set; }
        public DbSet<TransportRoute> TransportRoute { get; set; }
        public DbSet<TransportRouteHistory> TransportRouteHistory { get; set; }
        public DbSet<DepartmentUser> DepartmentUser { get; set; }
        public DbSet<Administor> Administor { get; set; }
        public DbSet<Office> Office { get; set; }
        public DbSet<BankBranch> BankBranch { get; set; }
        public DbSet<BankUser> BankUser { get; set; }
        public DbSet<Designation> Designation { get; set; }
        public DbSet<GatePass> GatePass { get; set; }
        public DbSet<UploadFileInfo> UploadFileInfo { get; set; }
        public DbSet<UploadFileInfoHistory> UploadFileInfoHistory { get; set; }
        public DbSet<DepartmentBankOrganizationMap> DepartmentBankOrganizationMap { get; set; }
        public DbSet<CommodityMaster> CommodityMaster { get; set; }
        public DbSet<UserFileDirectory> UserFileDirectory { get; set; }
        public DbSet<Model.Routes.Route> GetRoute()
        {
            return route;
        }

        public void SetRoute(DbSet<Model.Routes.Route> value)
        {
            route = value;
        }

        public DbSet<ExceptionLog> ExceptionLogs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //time zone
            modelBuilder.Entity<User>()
           .Property(e => e.CreatedOn)
           .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<UserFileDirectory>()
           .Property(e => e.CreatedDate)
           .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<UserLogin>()
            .Property(e => e.LoginTime)
            .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<User>()
            .Property(e => e.UpdatedOn)
            .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<User>()
         .Property(e => e.LastLogin)
         .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<Vehicle>()
        .Property(e => e.CreatedDate)
        .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<Vehicle>()
        .Property(e => e.UpdateDate)
        .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<DriverDetails>()
        .Property(e => e.LicenseExpiryDate)
        .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<DriverDetails>()
        .Property(e => e.DateOfBirth)
        .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<VehicleDriverMap>()
        .Property(e => e.AssignedDate)
        .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<VehicleDriverMap>()
        .Property(e => e.UnassignedDate)
        .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<TransportRoute>()
        .Property(e => e.ActualJourneyEnd)
        .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<TransportRoute>()
        .Property(e => e.CreatedDate)
        .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<TransportRoute>()
        .Property(e => e.UpdatedDate)
        .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<TransportRoute>()
        .Property(e => e.ExpectedJourneyEnd)
        .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<TransportRoute>()
        .Property(e => e.ExpectedJourneyStart)
        .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<TransportRoute>()
        .Property(e => e.ActualJourneyStart)
        .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<TransportRouteHistory>()
        .Property(e => e.ActionDate)
        .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<GatePass>()
        .Property(e => e.ArrivalTime)
        .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<GatePass>()
        .Property(e => e.DepartureTime)
        .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<GatePass>()
        .Property(e => e.IssueDate)
        .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<UploadFileInfo>()
     .Property(e => e.InprocessDate)
     .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<UploadFileInfo>()
        .Property(e => e.RequestedDate)
        .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<UploadFileInfo>()
        .Property(e => e.ResponseDate)
        .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<UploadFileInfoHistory>()
        .Property(e => e.ActionDate)
        .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<UserRole>()
            .HasData(
           new UserRole { Id = 1, RoleCode = "XVR2002", RoleName = "Supreme", RoleLevel = RoleLevels.SUPREME, RoleType = null }
           );
            modelBuilder.Entity<State>()
            .HasData(new State { Id = 1, StateCode = "XVR2002", StateName = "Haryana", StateLGDCode = "6" });
            modelBuilder.Entity<District>()
            .HasData(new District { Id = 1, DistrictCode = "XVR2002", DistrictName = "Ambala", DistrictLGDCode = "133", StateCode = "XVR2002" });
            modelBuilder.Entity<Ctv>()
            .HasData(new Ctv { Id = 1, CtvCode = "XVR2002", CtvName = "Saha", CtvLGDCode = "32", DistrictCode = "XVR2002" });
            modelBuilder.Entity<District>()
            .HasOne(ura => ura.State)
            .WithMany()
            .HasForeignKey(ura => ura.StateCode);
            modelBuilder.Entity<Ctv>()
             .HasOne(ura => ura.District)
             .WithMany()
             .HasForeignKey(ura => ura.DistrictCode);
            modelBuilder.Entity<Office>().HasOne(ura => ura.Organization).WithMany().HasForeignKey(ura => ura.OrganizationCode);
            modelBuilder.Entity<BankBranch>().HasOne(ura => ura.Organization).WithMany().HasForeignKey(ura => ura.OrganizationCode);
            modelBuilder.Entity<BankUser>().HasOne(ura => ura.User).WithMany().HasForeignKey(ura => ura.UserCode);
            modelBuilder.Entity<BankUser>().HasOne(ura => ura.UserRole).WithMany().HasForeignKey(ura => ura.RoleCode);
            modelBuilder.Entity<BankUser>().HasOne(ura => ura.Designation).WithMany().HasForeignKey(ura => ura.DesignationCode);
            modelBuilder.Entity<BankUser>().HasOne(ura => ura.BankBranch)
            .WithMany()
            .HasForeignKey(ura => ura.BankBranchCode);
            modelBuilder.Entity<Administor>()
           .HasOne(ura => ura.Organization)
           .WithMany()
           .HasForeignKey(ura => ura.OrganizationCode);
            modelBuilder.Entity<Administor>()
            .HasOne(ura => ura.UserRole)
            .WithMany()
            .HasForeignKey(ura => ura.RoleCode);
            modelBuilder.Entity<Administor>()
               .HasOne(ura => ura.User)
               .WithMany()
               .HasForeignKey(ura => ura.UserCode);
            modelBuilder.Entity<DepartmentUser>()
            .HasOne(ura => ura.Office)
            .WithMany()
            .HasForeignKey(ura => ura.OfficeCode);
            modelBuilder.Entity<DepartmentUser>()
            .HasOne(ura => ura.Designation)
            .WithMany()
            .HasForeignKey(ura => ura.DesignationCode);
            modelBuilder.Entity<DepartmentUser>()
             .HasOne(ura => ura.UserRole)
             .WithMany()
             .HasForeignKey(ura => ura.RoleCode);
            modelBuilder.Entity<DepartmentUser>()
               .HasOne(ura => ura.User)
               .WithMany()
               .HasForeignKey(ura => ura.UserCode);
            modelBuilder.Entity<TransporterUser>()
              .HasOne(ura => ura.User)
              .WithMany()
              .HasForeignKey(ura => ura.UserCode);
            modelBuilder.Entity<TransporterUser>()
            .HasOne(ura => ura.Organization)
            .WithMany()
            .HasForeignKey(ura => ura.OrganizationCode);
            modelBuilder.Entity<TransporterUser>()
           .HasOne(ura => ura.UserRole)
           .WithMany()
           .HasForeignKey(ura => ura.RoleCode);
            modelBuilder.Entity<RoleAccess>()
           .HasOne(ura => ura.User)
           .WithMany()
           .HasForeignKey(ura => ura.UserCode);

            modelBuilder.Entity<RoleAccess>()
             .HasOne(ura => ura.UserRole)
             .WithMany()
             .HasForeignKey(ura => ura.RoleCode);
            modelBuilder.Entity<MenuAccess>()
           .HasOne(c => c.UserRole)
           .WithMany()
           .HasForeignKey(c => c.RoleCode);

            modelBuilder.Entity<MenuAccess>()
            .HasOne(c => c.Menu)
            .WithMany()
            .HasForeignKey(c => c.MenuCode);

            modelBuilder.Entity<RouteAccess>()
            .HasOne(c => c.UserRole)
            .WithMany()
            .HasForeignKey(c => c.RoleCode);

            modelBuilder.Entity<RouteAccess>()
            .HasOne(c => c.Route)
            .WithMany()
            .HasForeignKey(c => c.RouteCode);

            modelBuilder.Entity<User>()
            .HasOne(c => c.Ctv)
            .WithMany()
            .HasForeignKey(c => c.CtvCode);

            modelBuilder.Entity<UserLogin>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserCode);
            modelBuilder.Entity<DriverDetails>()
          .HasOne(c => c.User)
          .WithMany()
          .HasForeignKey(c => c.UserCode);

            modelBuilder.Entity<Vehicle>()
            .HasOne(c => c.VehicleType)
            .WithMany()
            .HasForeignKey(c => c.VehicleTypeCode);

            modelBuilder.Entity<Vehicle>()
            .HasOne(c => c.Organization)
            .WithMany()
            .HasForeignKey(c => c.OrganizationCode);
            modelBuilder.Entity<VehicleDriverMap>()
       .HasOne(c => c.Vehicle)
       .WithMany()
       .HasForeignKey(c => c.VehicleCode);
            modelBuilder.Entity<VehicleDriverMap>()
            .HasOne(c => c.DriverDetail)
            .WithMany()
            .HasForeignKey(c => c.DriverDetailCode);
            modelBuilder.Entity<TransportRoute>()
       .HasOne(c => c.Vehicle)
       .WithMany()
       .HasForeignKey(c => c.VehicleCode);
            modelBuilder.Entity<TransportRoute>()
         .HasOne(c => c.User)
         .WithMany()
         .HasForeignKey(c => c.UserCode);
            modelBuilder.Entity<TransportRoute>()
     .Property(e => e.Commodities)
     .HasColumnType("jsonb");
            modelBuilder.Entity<TransportRouteHistory>()
         .HasOne(c => c.TransportRoute)
         .WithMany()
         .HasForeignKey(c => c.TransportRouteCode);
            modelBuilder.Entity<GatePass>()
                .HasOne(c => c.TransportRoute)
                .WithMany()
                .HasForeignKey(c => c.TransportRouteCode);

            modelBuilder.Entity<UploadFileInfo>()
         .HasOne(c => c.User)
         .WithMany()
         .HasForeignKey(c => c.UserCode);
            modelBuilder.Entity<UploadFileInfo>()
         .HasOne(c => c.Organization)
         .WithMany()
         .HasForeignKey(c => c.OrganizationCode);
            modelBuilder.Entity<UploadFileInfo>()
.Property(e => e.SharedUsers)
.HasColumnType("jsonb");
            modelBuilder.Entity<UploadFileInfoHistory>()
         .HasOne(c => c.UploadFileInfo)
         .WithMany()
         .HasForeignKey(c => c.FileInfoCode);
            modelBuilder.Entity<DepartmentBankOrganizationMap>()
      .HasOne(c => c.DepartmentOrganization)
      .WithMany()
      .HasForeignKey(c => c.DepartmentOrganizationCode);
            modelBuilder.Entity<DepartmentBankOrganizationMap>()
         .HasOne(c => c.BankOrganization)
         .WithMany()
         .HasForeignKey(c => c.BankOrganizationCode);


        }
    }
}

