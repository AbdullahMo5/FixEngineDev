using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FixEngine.Migrations
{
    /// <inheritdoc />
    public partial class addRelationbewtweenRiskUserAndAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Symbols",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "AppUserId",
                table: "RiskUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AppUserId1",
                table: "RiskUsers",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_RiskUsers_AppUserId1",
                table: "RiskUsers",
                column: "AppUserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_RiskUsers_AspNetUsers_AppUserId1",
                table: "RiskUsers",
                column: "AppUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RiskUsers_AspNetUsers_AppUserId1",
                table: "RiskUsers");

            migrationBuilder.DropIndex(
                name: "IX_RiskUsers_AppUserId1",
                table: "RiskUsers");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "RiskUsers");

            migrationBuilder.DropColumn(
                name: "AppUserId1",
                table: "RiskUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Symbols",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);
        }
    }
}
