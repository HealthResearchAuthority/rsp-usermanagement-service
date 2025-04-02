using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rsp.UsersService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateUserAuditTrail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserAuditTrails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SystemAdministratorId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAuditTrails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAuditTrails_Users_SystemAdministratorId",
                        column: x => x.SystemAdministratorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserAuditTrails_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAuditTrails_SystemAdministratorId",
                table: "UserAuditTrails",
                column: "SystemAdministratorId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAuditTrails_UserId",
                table: "UserAuditTrails",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAuditTrails");
        }
    }
}