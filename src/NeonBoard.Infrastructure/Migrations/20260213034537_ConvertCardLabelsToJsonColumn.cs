using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeonBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConvertCardLabelsToJsonColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardLabels");

            migrationBuilder.AddColumn<string>(
                name: "LabelIds",
                table: "Cards",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LabelIds",
                table: "Cards");

            migrationBuilder.CreateTable(
                name: "CardLabels",
                columns: table => new
                {
                    CardId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabelId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardLabels", x => new { x.CardId, x.LabelId });
                    table.ForeignKey(
                        name: "FK_CardLabels_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CardLabels_LabelId",
                table: "CardLabels",
                column: "LabelId");
        }
    }
}
