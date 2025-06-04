using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rsp.UsersService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "0755c2be-f404-44ed-bd78-93dca3b837a2");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "ddac109a-e4ad-4572-a175-7795f81ec0e7",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "system_administrator", "SYSTEM_ADMINISTRATOR" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "ddac109a-e4ad-4572-a175-7795f81ec0e7",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "admin", "ADMIN" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "0755c2be-f404-44ed-bd78-93dca3b837a2", "36ec8b0b-bf1b-4151-8978-e058ccf9d08c", "question_set_admin", "QUESTION_SET_ADMIN" });
        }
    }
}