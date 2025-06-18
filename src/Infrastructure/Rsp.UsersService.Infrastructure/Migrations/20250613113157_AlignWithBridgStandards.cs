using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rsp.UsersService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlignWithBridgStandards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Users",
                newName: "GivenName");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Users",
                newName: "FamilyName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GivenName",
                table: "Users",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "FamilyName",
                table: "Users",
                newName: "FirstName");
        }
    }
}
