using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LLMLab.Server.Migrations
{
    /// <inheritdoc />
    public partial class ReverseAttachmentRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM AttachementContent");
            migrationBuilder.Sql("DELETE FROM MessageAttachments");
            
            migrationBuilder.DropForeignKey(
                name: "FK_AttachementContent_MessageAttachments_AttachmentId",
                table: "AttachementContent");

            migrationBuilder.DropIndex(
                name: "IX_AttachementContent_AttachmentId",
                table: "AttachementContent");

            migrationBuilder.DropColumn(
                name: "AttachmentId",
                table: "AttachementContent");

            migrationBuilder.AddColumn<int>(
                name: "ContentId",
                table: "MessageAttachments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MessageAttachments_ContentId",
                table: "MessageAttachments",
                column: "ContentId");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageAttachments_AttachementContent_ContentId",
                table: "MessageAttachments",
                column: "ContentId",
                principalTable: "AttachementContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageAttachments_AttachementContent_ContentId",
                table: "MessageAttachments");

            migrationBuilder.DropIndex(
                name: "IX_MessageAttachments_ContentId",
                table: "MessageAttachments");

            migrationBuilder.DropColumn(
                name: "ContentId",
                table: "MessageAttachments");

            migrationBuilder.AddColumn<int>(
                name: "AttachmentId",
                table: "AttachementContent",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AttachementContent_AttachmentId",
                table: "AttachementContent",
                column: "AttachmentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AttachementContent_MessageAttachments_AttachmentId",
                table: "AttachementContent",
                column: "AttachmentId",
                principalTable: "MessageAttachments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
