using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LLMLab.Server.Migrations
{
    /// <inheritdoc />
    public partial class MessageMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MessageMetadata",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InputTokens = table.Column<int>(type: "int", nullable: false),
                    ThinkingTokens = table.Column<int>(type: "int", nullable: false),
                    OutputTokens = table.Column<int>(type: "int", nullable: false),
                    InputTokenCost = table.Column<double>(type: "float", nullable: false),
                    ThinkingTokenCost = table.Column<double>(type: "float", nullable: false),
                    OutputTokenCost = table.Column<double>(type: "float", nullable: false),
                    HasError = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModelProvider = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModelId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedUserApiKey = table.Column<bool>(type: "bit", nullable: false),
                    ResponseTimeSeconds = table.Column<double>(type: "float", nullable: false),
                    TotalInputTokenCost = table.Column<double>(type: "float", nullable: false),
                    TotalThinkingTokenCost = table.Column<double>(type: "float", nullable: false),
                    TotalOutputTokenCost = table.Column<double>(type: "float", nullable: false),
                    TotalCost = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageMetadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageMetadata_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessageMetadata_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageMetadata_MessageId",
                table: "MessageMetadata",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageMetadata_UserId",
                table: "MessageMetadata",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageMetadata");
        }
    }
}
