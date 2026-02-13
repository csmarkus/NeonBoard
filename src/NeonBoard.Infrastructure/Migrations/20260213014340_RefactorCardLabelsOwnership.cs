using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeonBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorCardLabelsOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CardLabels_Boards_BoardId",
                table: "CardLabels");

            migrationBuilder.DropIndex(
                name: "IX_CardLabels_BoardId",
                table: "CardLabels");

            migrationBuilder.DropIndex(
                name: "IX_CardLabels_CardId",
                table: "CardLabels");

            migrationBuilder.DropColumn(
                name: "BoardId",
                table: "CardLabels");

            migrationBuilder.CreateIndex(
                name: "IX_CardLabels_LabelId",
                table: "CardLabels",
                column: "LabelId");

            migrationBuilder.AddForeignKey(
                name: "FK_CardLabels_Cards_CardId",
                table: "CardLabels",
                column: "CardId",
                principalTable: "Cards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CardLabels_Cards_CardId",
                table: "CardLabels");

            migrationBuilder.DropIndex(
                name: "IX_CardLabels_LabelId",
                table: "CardLabels");

            migrationBuilder.AddColumn<Guid>(
                name: "BoardId",
                table: "CardLabels",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_CardLabels_BoardId",
                table: "CardLabels",
                column: "BoardId");

            migrationBuilder.CreateIndex(
                name: "IX_CardLabels_CardId",
                table: "CardLabels",
                column: "CardId");

            migrationBuilder.AddForeignKey(
                name: "FK_CardLabels_Boards_BoardId",
                table: "CardLabels",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
