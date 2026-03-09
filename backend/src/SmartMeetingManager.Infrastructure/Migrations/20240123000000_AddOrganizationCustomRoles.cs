using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMeetingManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationCustomRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrganizationCustomRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PermissionsJson = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationCustomRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationCustomRoles_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationCustomRoles_OrganizationId",
                table: "OrganizationCustomRoles",
                column: "OrganizationId");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomRoleId",
                table: "OrganizationMembers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomRoleId",
                table: "Invites",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationMembers_CustomRoleId",
                table: "OrganizationMembers",
                column: "CustomRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Invites_CustomRoleId",
                table: "Invites",
                column: "CustomRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationMembers_OrganizationCustomRoles_CustomRoleId",
                table: "OrganizationMembers",
                column: "CustomRoleId",
                principalTable: "OrganizationCustomRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Invites_OrganizationCustomRoles_CustomRoleId",
                table: "Invites",
                column: "CustomRoleId",
                principalTable: "OrganizationCustomRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationMembers_OrganizationCustomRoles_CustomRoleId",
                table: "OrganizationMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Invites_OrganizationCustomRoles_CustomRoleId",
                table: "Invites");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationMembers_CustomRoleId",
                table: "OrganizationMembers");

            migrationBuilder.DropIndex(
                name: "IX_Invites_CustomRoleId",
                table: "Invites");

            migrationBuilder.DropColumn(
                name: "CustomRoleId",
                table: "OrganizationMembers");

            migrationBuilder.DropColumn(
                name: "CustomRoleId",
                table: "Invites");

            migrationBuilder.DropTable(
                name: "OrganizationCustomRoles");
        }
    }
}
