using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeonBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserToActivityEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add columns as nullable
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "ActivityEntries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "ActivityEntries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            // Step 2: Backfill existing rows from Board → Project → Owner
            migrationBuilder.Sql("""
                UPDATE "ActivityEntries" ae
                SET "UserId" = p."OwnerId",
                    "UserName" = u."DisplayName"
                FROM "Boards" b
                INNER JOIN "Projects" p ON b."ProjectId" = p."Id"
                INNER JOIN "Users" u ON p."OwnerId" = u."Id"
                WHERE ae."BoardId" = b."Id"
                """);

            // Step 3: Make columns NOT NULL
            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "ActivityEntries",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "ActivityEntries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            // Step 4: Add index and FK
            migrationBuilder.CreateIndex(
                name: "IX_ActivityEntries_UserId",
                table: "ActivityEntries",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityEntries_Users_UserId",
                table: "ActivityEntries",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityEntries_Users_UserId",
                table: "ActivityEntries");

            migrationBuilder.DropIndex(
                name: "IX_ActivityEntries_UserId",
                table: "ActivityEntries");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ActivityEntries");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "ActivityEntries");
        }
    }
}
