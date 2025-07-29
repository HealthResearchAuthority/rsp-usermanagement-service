using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rsp.UsersService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FirstAndLastNamesSwappedForUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FamilyNameTemp",
                table: "Users",
                newName: "FamilyName");

            migrationBuilder.RenameColumn(
                name: "GivenNameTemp",
                table: "Users",
                newName: "GivenName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FamilyNameTemp",
                table: "Users",
                newName: "FamilyName");

            migrationBuilder.RenameColumn(
                name: "GivenNameTemp",
                table: "Users",
                newName: "GivenName");
        }
    }
}