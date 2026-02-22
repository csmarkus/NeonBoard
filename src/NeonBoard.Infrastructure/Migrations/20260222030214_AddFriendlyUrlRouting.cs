using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeonBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFriendlyUrlRouting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add columns as nullable first so existing rows don't violate NOT NULL
            migrationBuilder.AddColumn<string>(
                name: "ShortId",
                table: "Projects",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Boards",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            // Populate ShortId for existing projects using random 7-char hex strings
            migrationBuilder.Sql(@"
                UPDATE ""Projects""
                SET ""ShortId"" = substring(md5(random()::text) from 1 for 7)
                WHERE ""ShortId"" IS NULL OR ""ShortId"" = '';
            ");

            // Populate Slug for existing boards derived from board name
            migrationBuilder.Sql(@"
                UPDATE ""Boards""
                SET ""Slug"" = lower(regexp_replace(regexp_replace(""Name"", '[^a-zA-Z0-9]+', '-', 'g'), '^-|-$', '', 'g'))
                WHERE ""Slug"" IS NULL OR ""Slug"" = '';
            ");

            // Now make columns NOT NULL
            migrationBuilder.AlterColumn<string>(
                name: "ShortId",
                table: "Projects",
                type: "character varying(7)",
                maxLength: 7,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(7)",
                oldMaxLength: 7,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Boards",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            // Create unique indexes after data is populated
            migrationBuilder.CreateIndex(
                name: "IX_Projects_ShortId",
                table: "Projects",
                column: "ShortId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Boards_ProjectId_Slug",
                table: "Boards",
                columns: new[] { "ProjectId", "Slug" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Projects_ShortId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Boards_ProjectId_Slug",
                table: "Boards");

            migrationBuilder.DropColumn(
                name: "ShortId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Boards");
        }
    }
}
