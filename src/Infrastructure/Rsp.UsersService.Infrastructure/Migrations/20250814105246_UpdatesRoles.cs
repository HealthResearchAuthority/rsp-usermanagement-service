using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Rsp.UsersService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatesRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "b008864d-5781-47ee-9aa9-e204155198a3");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "c40073a2-8c05-41f9-a2e1-ce376b585b61");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "043aca8e-f88e-473e-974c-262f846285ea",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "study-wide_reviewer", "STUDY-WIDE_REVIEWER" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "043aca8e-f88e-473e-974c-262f846285ea",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "study_wide_reviewer", "STUDY_WIDE_REVIEWER" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "b008864d-5781-47ee-9aa9-e204155198a3", "133fb613-7a85-44f9-b461-d506cfabc2a2", "operations", "OPERATIONS" },
                    { "c40073a2-8c05-41f9-a2e1-ce376b585b61", "ad0bbca0-4d40-418d-8061-2904a29c2e4b", "reviewer", "REVIEWER" }
                });
        }
    }
}
