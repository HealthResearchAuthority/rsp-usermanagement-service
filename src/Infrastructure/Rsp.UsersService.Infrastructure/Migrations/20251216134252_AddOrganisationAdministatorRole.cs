using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rsp.UsersService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganisationAdministatorRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "7b6f1b5e-3a42-4a3f-9c6a-6f8a6f8c4d21", "c4e6b8a1-2a0f-4f6e-9b9a-3c6c6e4d9b87", "organisation_administrator", "ORGANISATION_ADMINISTRATOR" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "7b6f1b5e-3a42-4a3f-9c6a-6f8a6f8c4d21");
        }
    }
}
