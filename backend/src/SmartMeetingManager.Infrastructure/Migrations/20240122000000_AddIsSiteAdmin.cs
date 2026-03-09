using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMeetingManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsSiteAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSiteAdmin",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSiteAdmin",
                table: "Users");
        }
    }
}
