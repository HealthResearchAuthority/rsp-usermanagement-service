using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rsp.UsersService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CustomIdentitySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("IX_UserLogins_UserId", "Users");

            migrationBuilder.DropTable(name: "UserLogins");
            migrationBuilder.DropTable(name: "UserTokens");

            migrationBuilder.DropIndex("UserNameIndex", "Users");

            migrationBuilder.DropColumn("UserName", "Users");
            migrationBuilder.DropColumn("NormalizedUserName", "Users");
            migrationBuilder.DropColumn("EmailConfirmed", "Users");
            migrationBuilder.DropColumn("PasswordHash", "Users");
            migrationBuilder.DropColumn("PhoneNumber", "Users");
            migrationBuilder.DropColumn("PhoneNumberConfirmed", "Users");
            migrationBuilder.DropColumn("TwoFactorEnabled", "Users");
            migrationBuilder.DropColumn("LockoutEnd", "Users");
            migrationBuilder.DropColumn("LockoutEnabled", "Users");
            migrationBuilder.DropColumn("AccessFailedCount", "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
               name: "UserLogins",
               columns: table => new
               {
                   LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                   ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                   ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
               },
               constraints: table =>
               {
                   table.PrimaryKey("PK_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                   table.ForeignKey(
                       name: "FK_UserLogins_Users_UserId",
                       column: x => x.UserId,
                       principalTable: "Users",
                       principalColumn: "Id",
                       onDelete: ReferentialAction.Cascade);
               });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddColumn<string>("UserName", "Users", type: "nvarchar(256)", maxLength: 256, nullable: true);
            migrationBuilder.AddColumn<string>("NormalizedUserName", "Users", type: "nvarchar(256)", maxLength: 256, nullable: true);
            migrationBuilder.AddColumn<bool>("EmailConfirmed", "Users", type: "bit", nullable: false);
            migrationBuilder.AddColumn<string>("PasswordHash", "Users", type: "nvarchar(max)", nullable: true);
            migrationBuilder.AddColumn<string>("PhoneNumber", "Users", type: "nvarchar(max)", nullable: true);
            migrationBuilder.AddColumn<bool>("PhoneNumberConfirmed", "Users", type: "bit", nullable: false);
            migrationBuilder.AddColumn<bool>("TwoFactorEnabled", "Users", type: "bit", nullable: false);
            migrationBuilder.AddColumn<DateTimeOffset>("LockoutEnd", "Users", type: "datetimeoffset", nullable: true);
            migrationBuilder.AddColumn<bool>("LockoutEnabled", "Users", type: "bit", nullable: false);
            migrationBuilder.AddColumn<int>("AccessFailedCount", "Users", type: "int", nullable: false);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogins_UserId",
                table: "UserLogins",
                column: "UserId");
        }
    }
}