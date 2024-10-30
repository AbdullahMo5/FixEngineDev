using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FixEngine.Migrations
{
    /// <inheritdoc />
    public partial class updateAppuserIdToBeString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RiskUsers_AspNetUsers_AppUserId1",
                table: "RiskUsers");

            migrationBuilder.DropIndex(
                name: "IX_RiskUsers_AppUserId1",
                table: "RiskUsers");

            migrationBuilder.DropColumn(
                name: "AppUserId1",
                table: "RiskUsers");

            migrationBuilder.AlterColumn<string>(
                name: "AppUserId",
                table: "RiskUsers",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_RiskUsers_AppUserId",
                table: "RiskUsers",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RiskUsers_AspNetUsers_AppUserId",
                table: "RiskUsers",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RiskUsers_AspNetUsers_AppUserId",
                table: "RiskUsers");

            migrationBuilder.DropIndex(
                name: "IX_RiskUsers_AppUserId",
                table: "RiskUsers");

            migrationBuilder.AlterColumn<int>(
                name: "AppUserId",
                table: "RiskUsers",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

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
    }
}
