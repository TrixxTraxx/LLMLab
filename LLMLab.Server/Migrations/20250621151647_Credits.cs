using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LLMLab.Server.Migrations
{
    /// <inheritdoc />
    public partial class Credits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotallyBoughtCredits",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsedCredits",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotallyBoughtCredits",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UsedCredits",
                table: "AspNetUsers");
        }
    }
}
