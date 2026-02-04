using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDriectory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserFileDirectoryCode",
                table: "UploadFileInfo",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserFileDirectory",
                columns: table => new
                {
                    UserFileDirectoryCode = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DirectoryName = table.Column<string>(type: "text", nullable: false),
                    ParentDirectoryCode = table.Column<string>(type: "text", nullable: true),
                    CreatedByUserCode = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFileDirectory", x => x.UserFileDirectoryCode);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFileDirectory");

            migrationBuilder.DropColumn(
                name: "UserFileDirectoryCode",
                table: "UploadFileInfo");
        }
    }
}
