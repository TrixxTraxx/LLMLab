using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LLMLab.Server.Migrations
{
    /// <inheritdoc />
    public partial class SomeModelChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PreviousMessageId",
                table: "Messages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_PreviousMessageId",
                table: "Messages",
                column: "PreviousMessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Messages_PreviousMessageId",
                table: "Messages",
                column: "PreviousMessageId",
                principalTable: "Messages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Messages_PreviousMessageId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_PreviousMessageId",
                table: "Messages");

            migrationBuilder.AlterColumn<int>(
                name: "PreviousMessageId",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
