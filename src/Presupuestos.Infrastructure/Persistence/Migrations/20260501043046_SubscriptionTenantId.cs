using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Presupuestos.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class SubscriptionTenantId : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "TenantId",
            table: "Subscriptions",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.Sql(
            """
            UPDATE s
            SET TenantId = u.TenantId
            FROM Subscriptions s
            INNER JOIN Users u ON s.UserId = u.Id
            WHERE u.TenantId IS NOT NULL
            """);

        migrationBuilder.Sql("DELETE FROM Subscriptions WHERE TenantId IS NULL");

        migrationBuilder.DropForeignKey(
            name: "FK_Subscriptions_Users_UserId",
            table: "Subscriptions");

        migrationBuilder.DropIndex(
            name: "IX_Subscriptions_UserId",
            table: "Subscriptions");

        migrationBuilder.DropColumn(
            name: "UserId",
            table: "Subscriptions");

        migrationBuilder.AlterColumn<Guid>(
            name: "TenantId",
            table: "Subscriptions",
            type: "uniqueidentifier",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Subscriptions_TenantId",
            table: "Subscriptions",
            column: "TenantId");

        migrationBuilder.AddForeignKey(
            name: "FK_Subscriptions_Tenants_TenantId",
            table: "Subscriptions",
            column: "TenantId",
            principalTable: "Tenants",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Subscriptions_Tenants_TenantId",
            table: "Subscriptions");

        migrationBuilder.DropIndex(
            name: "IX_Subscriptions_TenantId",
            table: "Subscriptions");

        migrationBuilder.AddColumn<Guid>(
            name: "UserId",
            table: "Subscriptions",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.Sql(
            """
            UPDATE s
            SET UserId = x.Id
            FROM Subscriptions s
            CROSS APPLY (
                SELECT TOP 1 usr.Id
                FROM Users usr
                WHERE usr.TenantId = s.TenantId
                ORDER BY usr.CreatedAt
            ) x
            """);

        migrationBuilder.Sql("DELETE FROM Subscriptions WHERE UserId IS NULL");

        migrationBuilder.DropColumn(
            name: "TenantId",
            table: "Subscriptions");

        migrationBuilder.AlterColumn<Guid>(
            name: "UserId",
            table: "Subscriptions",
            type: "uniqueidentifier",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Subscriptions_UserId",
            table: "Subscriptions",
            column: "UserId");

        migrationBuilder.AddForeignKey(
            name: "FK_Subscriptions_Users_UserId",
            table: "Subscriptions",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
