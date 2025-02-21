using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Rsp.UsersService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RolesDataSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IdentityUserLogin<string>_Users_UserId",
                table: "IdentityUserLogin<string>");

            migrationBuilder.DropForeignKey(
                name: "FK_IdentityUserToken<string>_Users_UserId",
                table: "IdentityUserToken<string>");

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "0755c2be-f404-44ed-bd78-93dca3b837a2", "36ec8b0b-bf1b-4151-8978-e058ccf9d08c", "question_set_admin", "QUESTION_SET_ADMIN" },
                    { "c40073a2-8c05-41f9-a2e1-ce376b585b61", "ad0bbca0-4d40-418d-8061-2904a29c2e4b", "reviewer", "REVIEWER" },
                    { "d0c76beb-b25f-4d1d-8be8-2bdf5a3a4c92", "8c403e3f-540b-4ab1-ac5a-e0bc3fc65f6e", "user", "USER" },
                    { "ddac109a-e4ad-4572-a175-7795f81ec0e7", "019438de-9097-43c4-90e1-241e60224fa4", "admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "0755c2be-f404-44ed-bd78-93dca3b837a2");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "c40073a2-8c05-41f9-a2e1-ce376b585b61");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "d0c76beb-b25f-4d1d-8be8-2bdf5a3a4c92");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "ddac109a-e4ad-4572-a175-7795f81ec0e7");

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityUserLogin<string>_Users_UserId",
                table: "IdentityUserLogin<string>",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityUserToken<string>_Users_UserId",
                table: "IdentityUserToken<string>",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}