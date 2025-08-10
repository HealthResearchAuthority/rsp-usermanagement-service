using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Rsp.UsersService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "d0c76beb-b25f-4d1d-8be8-2bdf5a3a4c92",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "applicant", "APPLICANT" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "043aca8e-f88e-473e-974c-262f846285ea", "6c3d23eb-4b5f-4643-b23a-0d2bd58c034d", "study_wide_reviewer", "STUDY_WIDE_REVIEWER" },
                    { "952697e4-3d60-4172-b7bf-ee5ed6a7a1c3", "2845d0bc-25c4-4c81-a72c-e3e29d5b371f", "team_manager", "TEAM_MANAGER" },
                    { "97977acf-0bd5-48c2-9cf5-bc3fd215e5bb", "514b807f-9b52-4e0e-8774-83c462e5012c", "workflow_co-ordinator", "WORKFLOW_CO-ORDINATOR" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "043aca8e-f88e-473e-974c-262f846285ea");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "952697e4-3d60-4172-b7bf-ee5ed6a7a1c3");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "97977acf-0bd5-48c2-9cf5-bc3fd215e5bb");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "d0c76beb-b25f-4d1d-8be8-2bdf5a3a4c92",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "user", "USER" });
        }
    }
}
