using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Rsp.UsersService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewRecRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "a3d5f7b9-2c4e-4d8f-9a1b-6c7d8e9f0a12", "c2d3e4f5-a6b7-4c8d-9e0f-1a2b3c4d5e6f", "research_ethics_committee_manager", "RESEARCH_ETHICS_COMMITTEE_MANAGER" },
                    { "b4e6a8c0-3d5f-4e9a-8b2c-7d8e9f0a1b23", "d3e4f5a6-b7c8-4d9e-0f1a-2b3c4d5e6f7a", "reviewer", "REVIEWER" },
                    { "e1a3f6d2-7c4b-4a8e-9f1d-2b3c4d5e6f70", "a9b8c7d6-e5f4-4a3b-9c8d-7e6f5a4b3c21", "administrator", "ADMINISTRATOR" },
                    { "f2b4c6d8-1e3a-4b7c-9d2f-5a6b7c8d9e01", "b1c2d3e4-f5a6-4b7c-8d9e-0a1b2c3d4e5f", "member_management", "MEMBER_MANAGEMENT" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "a3d5f7b9-2c4e-4d8f-9a1b-6c7d8e9f0a12");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "b4e6a8c0-3d5f-4e9a-8b2c-7d8e9f0a1b23");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "e1a3f6d2-7c4b-4a8e-9f1d-2b3c4d5e6f70");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "f2b4c6d8-1e3a-4b7c-9d2f-5a6b7c8d9e01");
        }
    }
}
