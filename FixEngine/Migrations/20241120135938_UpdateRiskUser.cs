using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FixEngine.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRiskUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Leverage",
                table: "RiskUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RiskUserId",
                table: "Positions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MarginCall",
                table: "Groups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StopOut",
                table: "Groups",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Leverage",
                table: "RiskUsers");

            migrationBuilder.DropColumn(
                name: "RiskUserId",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "MarginCall",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "StopOut",
                table: "Groups");
        }
    }
}
