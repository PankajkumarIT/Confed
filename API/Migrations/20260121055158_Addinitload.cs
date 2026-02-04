using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class Addinitload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommodityMaster",
                columns: table => new
                {
                    CommodityCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommodityMaster", x => x.CommodityCode);
                });

            migrationBuilder.CreateTable(
                name: "Designation",
                columns: table => new
                {
                    DesignationCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DesignationName = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Designation", x => x.DesignationCode);
                });

            migrationBuilder.CreateTable(
                name: "ExceptionLogs",
                columns: table => new
                {
                    DataCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    OccuredAt = table.Column<string>(type: "text", nullable: true),
                    StringException = table.Column<string>(type: "text", nullable: true),
                    StackTrace = table.Column<string>(type: "text", nullable: true),
                    InnerException = table.Column<string>(type: "text", nullable: true),
                    MethodName = table.Column<string>(type: "text", nullable: true),
                    ClassName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExceptionLogs", x => x.DataCode);
                });

            migrationBuilder.CreateTable(
                name: "Menu",
                columns: table => new
                {
                    MenuCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MenuName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Path = table.Column<string>(type: "text", nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<bool>(type: "boolean", nullable: false),
                    ParentCode = table.Column<string>(type: "text", nullable: true),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menu", x => x.MenuCode);
                });

            migrationBuilder.CreateTable(
                name: "Organization",
                columns: table => new
                {
                    OrganizationCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationName = table.Column<string>(type: "text", nullable: false),
                    ShortName = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: false),
                    ContactNumber = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    OrganizationType = table.Column<string>(type: "text", nullable: true),
                    ParentOrganizationCode = table.Column<string>(type: "text", nullable: true),
                    FetureType = table.Column<string>(type: "text", nullable: true),
                    GstNumber = table.Column<string>(type: "text", nullable: true),
                    PanNumber = table.Column<string>(type: "text", nullable: true),
                    StorageSize = table.Column<double>(type: "double precision", nullable: false),
                    UsedStorageSize = table.Column<double>(type: "double precision", nullable: false),
                    AllocateStorageSize = table.Column<double>(type: "double precision", nullable: false),
                    PaymentFileHeader = table.Column<string>(type: "text", nullable: true),
                    PaymentResponseFileHeader = table.Column<string>(type: "text", nullable: true),
                    PaymentAcknowledgment = table.Column<string>(type: "text", nullable: true),
                    PaymentNotAcknowledgement = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsConversion = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization", x => x.OrganizationCode);
                });

            migrationBuilder.CreateTable(
                name: "Route",
                columns: table => new
                {
                    RouteCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RouteName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Path = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<bool>(type: "boolean", nullable: false),
                    ParentCode = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Route", x => x.RouteCode);
                });

            migrationBuilder.CreateTable(
                name: "State",
                columns: table => new
                {
                    StateCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StateName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    StateLGDCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_State", x => x.StateCode);
                });

            migrationBuilder.CreateTable(
                name: "UnitMaster",
                columns: table => new
                {
                    UnitCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UnitName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UnitWeight = table.Column<double>(type: "double precision", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitMaster", x => x.UnitCode);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    RoleCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RoleLevel = table.Column<int>(type: "integer", nullable: false),
                    RoleType = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.RoleCode);
                });

            migrationBuilder.CreateTable(
                name: "VehicleType",
                columns: table => new
                {
                    VehicleTypeCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VehicleTypeName = table.Column<string>(type: "text", nullable: false),
                    MaxLoadCapacity = table.Column<double>(type: "double precision", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleType", x => x.VehicleTypeCode);
                });

            migrationBuilder.CreateTable(
                name: "BankBranch",
                columns: table => new
                {
                    BankBranchCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BankName = table.Column<string>(type: "text", nullable: false),
                    BranchName = table.Column<string>(type: "text", nullable: false),
                    IFSC = table.Column<string>(type: "text", nullable: false),
                    MICR = table.Column<string>(type: "text", nullable: false),
                    ContactPerson = table.Column<string>(type: "text", nullable: false),
                    ContactNumber = table.Column<string>(type: "text", nullable: true),
                    BranchAddress = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: false),
                    OrganizationCode = table.Column<string>(type: "text", nullable: false),
                    TotalStorageSize = table.Column<double>(type: "double precision", nullable: false),
                    UsedStorageSize = table.Column<double>(type: "double precision", nullable: false),
                    AllocateStorageSize = table.Column<double>(type: "double precision", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankBranch", x => x.BankBranchCode);
                    table.ForeignKey(
                        name: "FK_BankBranch_Organization_OrganizationCode",
                        column: x => x.OrganizationCode,
                        principalTable: "Organization",
                        principalColumn: "OrganizationCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentBankOrganizationMap",
                columns: table => new
                {
                    DepartmentBankOrganizationMapCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DepartmentOrganizationCode = table.Column<string>(type: "text", nullable: false),
                    BankOrganizationCode = table.Column<string>(type: "text", nullable: false),
                    InputFileHeader = table.Column<string>(type: "text", nullable: false),
                    IsMapped = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentBankOrganizationMap", x => x.DepartmentBankOrganizationMapCode);
                    table.ForeignKey(
                        name: "FK_DepartmentBankOrganizationMap_Organization_BankOrganization~",
                        column: x => x.BankOrganizationCode,
                        principalTable: "Organization",
                        principalColumn: "OrganizationCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepartmentBankOrganizationMap_Organization_DepartmentOrgani~",
                        column: x => x.DepartmentOrganizationCode,
                        principalTable: "Organization",
                        principalColumn: "OrganizationCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Office",
                columns: table => new
                {
                    OfficeCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OfficeName = table.Column<string>(type: "text", nullable: false),
                    ContactPersonName = table.Column<string>(type: "text", nullable: false),
                    ContactNumber = table.Column<string>(type: "text", nullable: false),
                    ContactEmail = table.Column<string>(type: "text", nullable: false),
                    OrganizationCode = table.Column<string>(type: "text", nullable: false),
                    OfficeAddress = table.Column<string>(type: "text", nullable: false),
                    TotalStorageSize = table.Column<double>(type: "double precision", nullable: false),
                    UsedStorageSize = table.Column<double>(type: "double precision", nullable: false),
                    AllocateStorageSize = table.Column<double>(type: "double precision", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Office", x => x.OfficeCode);
                    table.ForeignKey(
                        name: "FK_Office_Organization_OrganizationCode",
                        column: x => x.OrganizationCode,
                        principalTable: "Organization",
                        principalColumn: "OrganizationCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "District",
                columns: table => new
                {
                    DistrictCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DistrictName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DistrictLGDCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    StateCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_District", x => x.DistrictCode);
                    table.ForeignKey(
                        name: "FK_District_State_StateCode",
                        column: x => x.StateCode,
                        principalTable: "State",
                        principalColumn: "StateCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenuAccess",
                columns: table => new
                {
                    AccessCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MenuCode = table.Column<string>(type: "text", nullable: false),
                    RoleCode = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuAccess", x => x.AccessCode);
                    table.ForeignKey(
                        name: "FK_MenuAccess_Menu_MenuCode",
                        column: x => x.MenuCode,
                        principalTable: "Menu",
                        principalColumn: "MenuCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MenuAccess_UserRole_RoleCode",
                        column: x => x.RoleCode,
                        principalTable: "UserRole",
                        principalColumn: "RoleCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RouteAccess",
                columns: table => new
                {
                    AccessCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RouteCode = table.Column<string>(type: "text", nullable: false),
                    RoleCode = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteAccess", x => x.AccessCode);
                    table.ForeignKey(
                        name: "FK_RouteAccess_Route_RouteCode",
                        column: x => x.RouteCode,
                        principalTable: "Route",
                        principalColumn: "RouteCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RouteAccess_UserRole_RoleCode",
                        column: x => x.RoleCode,
                        principalTable: "UserRole",
                        principalColumn: "RoleCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vehicle",
                columns: table => new
                {
                    VehicleCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VehicleNumber = table.Column<string>(type: "text", nullable: false),
                    VehicleTypeCode = table.Column<string>(type: "text", nullable: false),
                    Manufacturer = table.Column<string>(type: "text", nullable: false),
                    RCNumber = table.Column<string>(type: "text", nullable: false),
                    ModelName = table.Column<string>(type: "text", nullable: false),
                    YearOfManufacture = table.Column<int>(type: "integer", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: true),
                    ChassisNumber = table.Column<string>(type: "text", nullable: false),
                    CapacityInTons = table.Column<double>(type: "double precision", nullable: false),
                    FuelTankCapacityInLiters = table.Column<double>(type: "double precision", nullable: false),
                    FuelType = table.Column<string>(type: "text", nullable: false),
                    GPSDeviceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    OrganizationCode = table.Column<string>(type: "text", nullable: false),
                    RatePerKm = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Createdby = table.Column<string>(type: "text", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicle", x => x.VehicleCode);
                    table.ForeignKey(
                        name: "FK_Vehicle_Organization_OrganizationCode",
                        column: x => x.OrganizationCode,
                        principalTable: "Organization",
                        principalColumn: "OrganizationCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vehicle_VehicleType_VehicleTypeCode",
                        column: x => x.VehicleTypeCode,
                        principalTable: "VehicleType",
                        principalColumn: "VehicleTypeCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ctv",
                columns: table => new
                {
                    CtvCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CtvName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CtvLGDCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    DistrictCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ctv", x => x.CtvCode);
                    table.ForeignKey(
                        name: "FK_Ctv_District_DistrictCode",
                        column: x => x.DistrictCode,
                        principalTable: "District",
                        principalColumn: "DistrictCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MobileNumber = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    EMail = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: false),
                    Address = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: false),
                    CtvCode = table.Column<string>(type: "text", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsEntityUser = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    LastLogin = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserCode);
                    table.ForeignKey(
                        name: "FK_User_Ctv_CtvCode",
                        column: x => x.CtvCode,
                        principalTable: "Ctv",
                        principalColumn: "CtvCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Administor",
                columns: table => new
                {
                    AdministorCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationCode = table.Column<string>(type: "text", nullable: false),
                    UserCode = table.Column<string>(type: "text", nullable: false),
                    RoleCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Administor", x => x.AdministorCode);
                    table.ForeignKey(
                        name: "FK_Administor_Organization_OrganizationCode",
                        column: x => x.OrganizationCode,
                        principalTable: "Organization",
                        principalColumn: "OrganizationCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Administor_UserRole_RoleCode",
                        column: x => x.RoleCode,
                        principalTable: "UserRole",
                        principalColumn: "RoleCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Administor_User_UserCode",
                        column: x => x.UserCode,
                        principalTable: "User",
                        principalColumn: "UserCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankUser",
                columns: table => new
                {
                    BankUserCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DesignationCode = table.Column<string>(type: "text", nullable: false),
                    BankBranchCode = table.Column<string>(type: "text", nullable: false),
                    UserCode = table.Column<string>(type: "text", nullable: false),
                    RoleCode = table.Column<string>(type: "text", nullable: false),
                    TotalStorageSize = table.Column<double>(type: "double precision", nullable: false),
                    UsedStorageSize = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankUser", x => x.BankUserCode);
                    table.ForeignKey(
                        name: "FK_BankUser_BankBranch_BankBranchCode",
                        column: x => x.BankBranchCode,
                        principalTable: "BankBranch",
                        principalColumn: "BankBranchCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BankUser_Designation_DesignationCode",
                        column: x => x.DesignationCode,
                        principalTable: "Designation",
                        principalColumn: "DesignationCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BankUser_UserRole_RoleCode",
                        column: x => x.RoleCode,
                        principalTable: "UserRole",
                        principalColumn: "RoleCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BankUser_User_UserCode",
                        column: x => x.UserCode,
                        principalTable: "User",
                        principalColumn: "UserCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentUser",
                columns: table => new
                {
                    DepartmentUserCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OfficeCode = table.Column<string>(type: "text", nullable: false),
                    UserCode = table.Column<string>(type: "text", nullable: false),
                    RoleCode = table.Column<string>(type: "text", nullable: false),
                    DesignationCode = table.Column<string>(type: "text", nullable: false),
                    TotalStorageSize = table.Column<double>(type: "double precision", nullable: false),
                    UsedStorageSize = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentUser", x => x.DepartmentUserCode);
                    table.ForeignKey(
                        name: "FK_DepartmentUser_Designation_DesignationCode",
                        column: x => x.DesignationCode,
                        principalTable: "Designation",
                        principalColumn: "DesignationCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepartmentUser_Office_OfficeCode",
                        column: x => x.OfficeCode,
                        principalTable: "Office",
                        principalColumn: "OfficeCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepartmentUser_UserRole_RoleCode",
                        column: x => x.RoleCode,
                        principalTable: "UserRole",
                        principalColumn: "RoleCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepartmentUser_User_UserCode",
                        column: x => x.UserCode,
                        principalTable: "User",
                        principalColumn: "UserCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DriverDetails",
                columns: table => new
                {
                    DriverDetailCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DriverName = table.Column<string>(type: "text", nullable: true),
                    LicenseNumber = table.Column<string>(type: "text", nullable: true),
                    LicenseType = table.Column<string>(type: "text", nullable: true),
                    LicenseExpiryDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EmergencyContact = table.Column<string>(type: "text", nullable: true),
                    BloodGroup = table.Column<string>(type: "text", nullable: true),
                    UserCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverDetails", x => x.DriverDetailCode);
                    table.ForeignKey(
                        name: "FK_DriverDetails_User_UserCode",
                        column: x => x.UserCode,
                        principalTable: "User",
                        principalColumn: "UserCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleAccess",
                columns: table => new
                {
                    AccessId = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserCode = table.Column<string>(type: "text", nullable: false),
                    RoleCode = table.Column<string>(type: "text", nullable: false),
                    AccessToRole = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleAccess", x => x.AccessId);
                    table.ForeignKey(
                        name: "FK_RoleAccess_UserRole_RoleCode",
                        column: x => x.RoleCode,
                        principalTable: "UserRole",
                        principalColumn: "RoleCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleAccess_User_UserCode",
                        column: x => x.UserCode,
                        principalTable: "User",
                        principalColumn: "UserCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransporterUser",
                columns: table => new
                {
                    TransportUserCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationCode = table.Column<string>(type: "text", nullable: false),
                    UserCode = table.Column<string>(type: "text", nullable: false),
                    IsDriver = table.Column<bool>(type: "boolean", nullable: false),
                    RoleCode = table.Column<string>(type: "text", nullable: false),
                    TotalStorageSize = table.Column<double>(type: "double precision", nullable: false),
                    UsedStorageSize = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransporterUser", x => x.TransportUserCode);
                    table.ForeignKey(
                        name: "FK_TransporterUser_Organization_OrganizationCode",
                        column: x => x.OrganizationCode,
                        principalTable: "Organization",
                        principalColumn: "OrganizationCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransporterUser_UserRole_RoleCode",
                        column: x => x.RoleCode,
                        principalTable: "UserRole",
                        principalColumn: "RoleCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransporterUser_User_UserCode",
                        column: x => x.UserCode,
                        principalTable: "User",
                        principalColumn: "UserCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransportRoute",
                columns: table => new
                {
                    TransportRouteCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SourceOrganizationCode = table.Column<string>(type: "text", nullable: false),
                    PickupAddress = table.Column<string>(type: "text", nullable: false),
                    DestinationAddress = table.Column<string>(type: "text", nullable: false),
                    DestinationContactNo = table.Column<string>(type: "text", nullable: false),
                    VehicleCode = table.Column<string>(type: "text", nullable: false),
                    Commodities = table.Column<string>(type: "jsonb", nullable: true),
                    Quantity = table.Column<double>(type: "double precision", nullable: false),
                    TotalWeight = table.Column<double>(type: "double precision", nullable: false),
                    DistanceInKm = table.Column<double>(type: "double precision", nullable: false),
                    ExpectedTravelTimeHours = table.Column<double>(type: "double precision", nullable: false),
                    ExpectedJourneyStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ExpectedJourneyEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ActualJourneyStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ActualJourneyEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    BaseAmount = table.Column<double>(type: "double precision", nullable: false),
                    TollTax = table.Column<double>(type: "double precision", nullable: false),
                    OtherCharges = table.Column<double>(type: "double precision", nullable: false),
                    TotalCharge = table.Column<double>(type: "double precision", nullable: false),
                    ApprovalStatus = table.Column<string>(type: "text", nullable: false),
                    DestinationStatus = table.Column<string>(type: "text", nullable: true),
                    UserCode = table.Column<string>(type: "text", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransportRoute", x => x.TransportRouteCode);
                    table.ForeignKey(
                        name: "FK_TransportRoute_User_UserCode",
                        column: x => x.UserCode,
                        principalTable: "User",
                        principalColumn: "UserCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransportRoute_Vehicle_VehicleCode",
                        column: x => x.VehicleCode,
                        principalTable: "Vehicle",
                        principalColumn: "VehicleCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UploadFileInfo",
                columns: table => new
                {
                    FileInfoCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileNumber = table.Column<string>(type: "text", nullable: true),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    BankName = table.Column<string>(type: "text", nullable: true),
                    BranchName = table.Column<string>(type: "text", nullable: true),
                    BankBranchCode = table.Column<string>(type: "text", nullable: true),
                    OfficeName = table.Column<string>(type: "text", nullable: true),
                    OfficeCode = table.Column<string>(type: "text", nullable: true),
                    IFSC = table.Column<string>(type: "text", nullable: true),
                    OrganizationCode = table.Column<string>(type: "text", nullable: false),
                    RequestFilePath = table.Column<string>(type: "text", nullable: true),
                    ResponseFilePath = table.Column<string>(type: "text", nullable: true),
                    InProcessFilePath = table.Column<string>(type: "text", nullable: true),
                    BankProcessFilePath = table.Column<string>(type: "text", nullable: true),
                    RejectFilePath = table.Column<string>(type: "text", nullable: true),
                    AcknowledgementFileNamePath = table.Column<string>(type: "text", nullable: true),
                    NoAcknowledgementFileNamePath = table.Column<string>(type: "text", nullable: true),
                    BankResponsePath = table.Column<string>(type: "text", nullable: true),
                    BankAcknowledgementFilePath = table.Column<string>(type: "text", nullable: true),
                    BankNoAcknowledgementFilePath = table.Column<string>(type: "text", nullable: true),
                    InternalFilePath = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    DepartmentApprovalStatus = table.Column<string>(type: "text", nullable: true),
                    IsInternalOnly = table.Column<bool>(type: "boolean", nullable: false),
                    RequestedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    InprocessDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ResponseDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ResponseFileName = table.Column<string>(type: "text", nullable: true),
                    AcknowledgementFileName = table.Column<string>(type: "text", nullable: true),
                    NoAcknowledgementFileName = table.Column<string>(type: "text", nullable: true),
                    ResponseUserCode = table.Column<string>(type: "text", nullable: true),
                    FileSize = table.Column<double>(type: "double precision", nullable: false),
                    ResponseOrganizationCode = table.Column<string>(type: "text", nullable: true),
                    FileType = table.Column<string>(type: "text", nullable: true),
                    TotalCount = table.Column<int>(type: "integer", nullable: false),
                    FailedCount = table.Column<int>(type: "integer", nullable: false),
                    ProcessedCount = table.Column<int>(type: "integer", nullable: false),
                    UserCode = table.Column<string>(type: "text", nullable: false),
                    IsPrivate = table.Column<bool>(type: "boolean", nullable: false),
                    SharedUsers = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadFileInfo", x => x.FileInfoCode);
                    table.ForeignKey(
                        name: "FK_UploadFileInfo_Organization_OrganizationCode",
                        column: x => x.OrganizationCode,
                        principalTable: "Organization",
                        principalColumn: "OrganizationCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UploadFileInfo_User_UserCode",
                        column: x => x.UserCode,
                        principalTable: "User",
                        principalColumn: "UserCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLogin",
                columns: table => new
                {
                    LoginCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LoginTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UserCode = table.Column<string>(type: "text", nullable: false),
                    Latitude = table.Column<string>(type: "text", nullable: true),
                    Longitude = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogin", x => x.LoginCode);
                    table.ForeignKey(
                        name: "FK_UserLogin_User_UserCode",
                        column: x => x.UserCode,
                        principalTable: "User",
                        principalColumn: "UserCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleDriverMap",
                columns: table => new
                {
                    VehicleDriverMapCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DriverDetailCode = table.Column<string>(type: "text", nullable: false),
                    VehicleCode = table.Column<string>(type: "text", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UnassignedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleDriverMap", x => x.VehicleDriverMapCode);
                    table.ForeignKey(
                        name: "FK_VehicleDriverMap_DriverDetails_DriverDetailCode",
                        column: x => x.DriverDetailCode,
                        principalTable: "DriverDetails",
                        principalColumn: "DriverDetailCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VehicleDriverMap_Vehicle_VehicleCode",
                        column: x => x.VehicleCode,
                        principalTable: "Vehicle",
                        principalColumn: "VehicleCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GatePass",
                columns: table => new
                {
                    GatePassCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TransportRouteCode = table.Column<string>(type: "text", nullable: false),
                    IssuedByUserCode = table.Column<string>(type: "text", nullable: true),
                    IssueDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DepartureTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArrivalTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    QRCodePath = table.Column<string>(type: "text", nullable: true),
                    GatePassFilePath = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GatePass", x => x.GatePassCode);
                    table.ForeignKey(
                        name: "FK_GatePass_TransportRoute_TransportRouteCode",
                        column: x => x.TransportRouteCode,
                        principalTable: "TransportRoute",
                        principalColumn: "TransportRouteCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransportRouteHistory",
                columns: table => new
                {
                    ApprovalCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TransportRouteCode = table.Column<string>(type: "text", nullable: false),
                    ActionByUserCode = table.Column<string>(type: "text", nullable: false),
                    ActionByRoleCode = table.Column<string>(type: "text", nullable: false),
                    AssignedToRoleCode = table.Column<string>(type: "text", nullable: true),
                    AssignedToUserCode = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ActionDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransportRouteHistory", x => x.ApprovalCode);
                    table.ForeignKey(
                        name: "FK_TransportRouteHistory_TransportRoute_TransportRouteCode",
                        column: x => x.TransportRouteCode,
                        principalTable: "TransportRoute",
                        principalColumn: "TransportRouteCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UploadFileInfoHistory",
                columns: table => new
                {
                    UploadFileInfoHistoryCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileInfoCode = table.Column<string>(type: "text", nullable: false),
                    ActionByUserCode = table.Column<string>(type: "text", nullable: false),
                    ActionByRoleCode = table.Column<string>(type: "text", nullable: false),
                    AssignedToRoleCode = table.Column<string>(type: "text", nullable: true),
                    AssignedToUserCode = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ActionDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadFileInfoHistory", x => x.UploadFileInfoHistoryCode);
                    table.ForeignKey(
                        name: "FK_UploadFileInfoHistory_UploadFileInfo_FileInfoCode",
                        column: x => x.FileInfoCode,
                        principalTable: "UploadFileInfo",
                        principalColumn: "FileInfoCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "State",
                columns: new[] { "StateCode", "Id", "StateLGDCode", "StateName" },
                values: new object[] { "XVR2002", 1, "6", "Haryana" });

            migrationBuilder.InsertData(
                table: "UserRole",
                columns: new[] { "RoleCode", "Id", "RoleLevel", "RoleName", "RoleType" },
                values: new object[] { "XVR2002", 1, 7, "Supreme", null });

            migrationBuilder.InsertData(
                table: "District",
                columns: new[] { "DistrictCode", "DistrictLGDCode", "DistrictName", "Id", "StateCode" },
                values: new object[] { "XVR2002", "133", "Ambala", 1, "XVR2002" });

            migrationBuilder.InsertData(
                table: "Ctv",
                columns: new[] { "CtvCode", "CtvLGDCode", "CtvName", "DistrictCode", "Id" },
                values: new object[] { "XVR2002", "32", "Saha", "XVR2002", 1 });

            migrationBuilder.CreateIndex(
                name: "IX_Administor_OrganizationCode",
                table: "Administor",
                column: "OrganizationCode");

            migrationBuilder.CreateIndex(
                name: "IX_Administor_RoleCode",
                table: "Administor",
                column: "RoleCode");

            migrationBuilder.CreateIndex(
                name: "IX_Administor_UserCode",
                table: "Administor",
                column: "UserCode");

            migrationBuilder.CreateIndex(
                name: "IX_BankBranch_OrganizationCode",
                table: "BankBranch",
                column: "OrganizationCode");

            migrationBuilder.CreateIndex(
                name: "IX_BankUser_BankBranchCode",
                table: "BankUser",
                column: "BankBranchCode");

            migrationBuilder.CreateIndex(
                name: "IX_BankUser_DesignationCode",
                table: "BankUser",
                column: "DesignationCode");

            migrationBuilder.CreateIndex(
                name: "IX_BankUser_RoleCode",
                table: "BankUser",
                column: "RoleCode");

            migrationBuilder.CreateIndex(
                name: "IX_BankUser_UserCode",
                table: "BankUser",
                column: "UserCode");

            migrationBuilder.CreateIndex(
                name: "IX_Ctv_DistrictCode",
                table: "Ctv",
                column: "DistrictCode");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentBankOrganizationMap_BankOrganizationCode",
                table: "DepartmentBankOrganizationMap",
                column: "BankOrganizationCode");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentBankOrganizationMap_DepartmentOrganizationCode",
                table: "DepartmentBankOrganizationMap",
                column: "DepartmentOrganizationCode");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentUser_DesignationCode",
                table: "DepartmentUser",
                column: "DesignationCode");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentUser_OfficeCode",
                table: "DepartmentUser",
                column: "OfficeCode");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentUser_RoleCode",
                table: "DepartmentUser",
                column: "RoleCode");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentUser_UserCode",
                table: "DepartmentUser",
                column: "UserCode");

            migrationBuilder.CreateIndex(
                name: "IX_District_StateCode",
                table: "District",
                column: "StateCode");

            migrationBuilder.CreateIndex(
                name: "IX_DriverDetails_UserCode",
                table: "DriverDetails",
                column: "UserCode");

            migrationBuilder.CreateIndex(
                name: "IX_GatePass_TransportRouteCode",
                table: "GatePass",
                column: "TransportRouteCode");

            migrationBuilder.CreateIndex(
                name: "IX_MenuAccess_MenuCode",
                table: "MenuAccess",
                column: "MenuCode");

            migrationBuilder.CreateIndex(
                name: "IX_MenuAccess_RoleCode",
                table: "MenuAccess",
                column: "RoleCode");

            migrationBuilder.CreateIndex(
                name: "IX_Office_OrganizationCode",
                table: "Office",
                column: "OrganizationCode");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAccess_RoleCode",
                table: "RoleAccess",
                column: "RoleCode");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAccess_UserCode",
                table: "RoleAccess",
                column: "UserCode");

            migrationBuilder.CreateIndex(
                name: "IX_RouteAccess_RoleCode",
                table: "RouteAccess",
                column: "RoleCode");

            migrationBuilder.CreateIndex(
                name: "IX_RouteAccess_RouteCode",
                table: "RouteAccess",
                column: "RouteCode");

            migrationBuilder.CreateIndex(
                name: "IX_TransporterUser_OrganizationCode",
                table: "TransporterUser",
                column: "OrganizationCode");

            migrationBuilder.CreateIndex(
                name: "IX_TransporterUser_RoleCode",
                table: "TransporterUser",
                column: "RoleCode");

            migrationBuilder.CreateIndex(
                name: "IX_TransporterUser_UserCode",
                table: "TransporterUser",
                column: "UserCode");

            migrationBuilder.CreateIndex(
                name: "IX_TransportRoute_UserCode",
                table: "TransportRoute",
                column: "UserCode");

            migrationBuilder.CreateIndex(
                name: "IX_TransportRoute_VehicleCode",
                table: "TransportRoute",
                column: "VehicleCode");

            migrationBuilder.CreateIndex(
                name: "IX_TransportRouteHistory_TransportRouteCode",
                table: "TransportRouteHistory",
                column: "TransportRouteCode");

            migrationBuilder.CreateIndex(
                name: "IX_UploadFileInfo_OrganizationCode",
                table: "UploadFileInfo",
                column: "OrganizationCode");

            migrationBuilder.CreateIndex(
                name: "IX_UploadFileInfo_UserCode",
                table: "UploadFileInfo",
                column: "UserCode");

            migrationBuilder.CreateIndex(
                name: "IX_UploadFileInfoHistory_FileInfoCode",
                table: "UploadFileInfoHistory",
                column: "FileInfoCode");

            migrationBuilder.CreateIndex(
                name: "IX_User_CtvCode",
                table: "User",
                column: "CtvCode");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogin_UserCode",
                table: "UserLogin",
                column: "UserCode");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_OrganizationCode",
                table: "Vehicle",
                column: "OrganizationCode");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_VehicleTypeCode",
                table: "Vehicle",
                column: "VehicleTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleDriverMap_DriverDetailCode",
                table: "VehicleDriverMap",
                column: "DriverDetailCode");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleDriverMap_VehicleCode",
                table: "VehicleDriverMap",
                column: "VehicleCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Administor");

            migrationBuilder.DropTable(
                name: "BankUser");

            migrationBuilder.DropTable(
                name: "CommodityMaster");

            migrationBuilder.DropTable(
                name: "DepartmentBankOrganizationMap");

            migrationBuilder.DropTable(
                name: "DepartmentUser");

            migrationBuilder.DropTable(
                name: "ExceptionLogs");

            migrationBuilder.DropTable(
                name: "GatePass");

            migrationBuilder.DropTable(
                name: "MenuAccess");

            migrationBuilder.DropTable(
                name: "RoleAccess");

            migrationBuilder.DropTable(
                name: "RouteAccess");

            migrationBuilder.DropTable(
                name: "TransporterUser");

            migrationBuilder.DropTable(
                name: "TransportRouteHistory");

            migrationBuilder.DropTable(
                name: "UnitMaster");

            migrationBuilder.DropTable(
                name: "UploadFileInfoHistory");

            migrationBuilder.DropTable(
                name: "UserLogin");

            migrationBuilder.DropTable(
                name: "VehicleDriverMap");

            migrationBuilder.DropTable(
                name: "BankBranch");

            migrationBuilder.DropTable(
                name: "Designation");

            migrationBuilder.DropTable(
                name: "Office");

            migrationBuilder.DropTable(
                name: "Menu");

            migrationBuilder.DropTable(
                name: "Route");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "TransportRoute");

            migrationBuilder.DropTable(
                name: "UploadFileInfo");

            migrationBuilder.DropTable(
                name: "DriverDetails");

            migrationBuilder.DropTable(
                name: "Vehicle");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Organization");

            migrationBuilder.DropTable(
                name: "VehicleType");

            migrationBuilder.DropTable(
                name: "Ctv");

            migrationBuilder.DropTable(
                name: "District");

            migrationBuilder.DropTable(
                name: "State");
        }
    }
}
