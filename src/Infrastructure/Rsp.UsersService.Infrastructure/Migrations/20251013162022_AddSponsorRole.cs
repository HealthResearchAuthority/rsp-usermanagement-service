using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rsp.UsersService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSponsorRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "c2c2b6c9-1f6c-4a0a-9d2a-6f2b7b4b1c8e", "7f3d1a2b-5e64-4f08-9b7a-3c6d2c4b8e9f", "sponsor", "SPONSOR" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "c2c2b6c9-1f6c-4a0a-9d2a-6f2b7b4b1c8e");
        }
    }
}
