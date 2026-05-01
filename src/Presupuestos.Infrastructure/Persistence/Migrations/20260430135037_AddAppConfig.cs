using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Presupuestos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAppConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppEnabled = table.Column<bool>(type: "bit", nullable: false),
                    MinimumVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BlockedVersions = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ForceUpdate = table.Column<bool>(type: "bit", nullable: false),
                    MaintenanceMode = table.Column<bool>(type: "bit", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppConfigs", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AppConfigs",
                columns: new[] { "Id", "AppEnabled", "MinimumVersion", "BlockedVersions", "ForceUpdate", "MaintenanceMode", "Message" },
                values: new object[]
                {
                    new Guid("00000000-0000-4000-8000-000000000001"),
                    true,
                    null,
                    "[]",
                    false,
                    false,
                    ""
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppConfigs");
        }
    }
}
