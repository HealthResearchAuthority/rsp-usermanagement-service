using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rsp.UsersService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FirstAndLastNamesSwappedForUsersChangeToTemp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GivenName",
                table: "Users",
                newName: "FamilyNameTemp");

            migrationBuilder.RenameColumn(
                name: "FamilyName",
                table: "Users",
                newName: "GivenNameTemp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FamilyNameTemp",
                table: "Users",
                newName: "GivenName");

            migrationBuilder.RenameColumn(
                name: "GivenNameTemp",
                table: "Users",
                newName: "FamilyName");
        }
    }
}