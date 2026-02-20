using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeonBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBoardPrefixAndCardNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cards_BoardId",
                table: "Cards");

            migrationBuilder.AddColumn<int>(
                name: "CardNumber",
                table: "Cards",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NextCardNumber",
                table: "Boards",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "Prefix",
                table: "Boards",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "");

            // Backfill prefix for existing boards
            migrationBuilder.Sql(@"
                UPDATE ""Boards""
                SET ""Prefix"" = UPPER(LEFT(REGEXP_REPLACE(""Name"", '[^a-zA-Z]', '', 'g'), 3))
                WHERE ""Prefix"" = '';

                UPDATE ""Boards""
                SET ""Prefix"" = RPAD(""Prefix"", 2, UPPER(LEFT(""Prefix"", 1)))
                WHERE LENGTH(""Prefix"") < 2;
            ");

            // Backfill card numbers by CreatedAt order within each board
            migrationBuilder.Sql(@"
                WITH numbered AS (
                    SELECT ""Id"", ""BoardId"",
                        ROW_NUMBER() OVER (PARTITION BY ""BoardId"" ORDER BY ""CreatedAt"") as rn
                    FROM ""Cards""
                )
                UPDATE ""Cards"" c
                SET ""CardNumber"" = n.rn
                FROM numbered n
                WHERE c.""Id"" = n.""Id"";
            ");

            // Set NextCardNumber to max card number + 1 (or 1 if no cards)
            migrationBuilder.Sql(@"
                UPDATE ""Boards"" b
                SET ""NextCardNumber"" = COALESCE(
                    (SELECT MAX(c.""CardNumber"") + 1 FROM ""Cards"" c WHERE c.""BoardId"" = b.""Id""),
                    1
                );
            ");

            // Create unique indexes after backfill to avoid constraint violations
            migrationBuilder.CreateIndex(
                name: "IX_Cards_BoardId_CardNumber",
                table: "Cards",
                columns: new[] { "BoardId", "CardNumber" },
                unique: true);

            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX ""IX_Boards_ProjectId_Prefix""
                ON ""Boards"" (""ProjectId"", ""Prefix"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Boards_ProjectId_Prefix",
                table: "Boards");

            migrationBuilder.DropIndex(
                name: "IX_Cards_BoardId_CardNumber",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "CardNumber",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "NextCardNumber",
                table: "Boards");

            migrationBuilder.DropColumn(
                name: "Prefix",
                table: "Boards");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_BoardId",
                table: "Cards",
                column: "BoardId");
        }
    }
}
