using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMeetingManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvitePassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InvitePasswordHash",
                table: "Invites",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvitePasswordHash",
                table: "Invites");
        }
    }
}
