using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeonBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FractionalIndexPositions : Migration
    {
        // Base-62 alphabet matching FractionalIndex.DIGITS.
        // Positions are mapped to 'a' + single base-62 digit, giving 62 positions per group
        // (more than enough for columns per board or cards per column).
        private const string DIGITS = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Change column types to varchar
            migrationBuilder.Sql("""
                ALTER TABLE "Columns" ALTER COLUMN "Position" TYPE character varying(50)
                USING "Position"::text;
            """);

            migrationBuilder.Sql("""
                ALTER TABLE "Cards" ALTER COLUMN "Position" TYPE character varying(50)
                USING "Position"::text;
            """);

            // Step 2: Convert integer positions to fractional index keys using row numbers
            // within each group. This normalizes any gaps (e.g., 0, 1, 5, 8 → a0, a1, a2, a3)
            // and ensures correct lexicographic sort order.
            migrationBuilder.Sql($"""
                WITH ranked AS (
                    SELECT "Id",
                           ROW_NUMBER() OVER (PARTITION BY "BoardId" ORDER BY "Position"::integer) - 1 AS pos
                    FROM "Columns"
                )
                UPDATE "Columns" c
                SET "Position" = 'a' || substr('{DIGITS}', (ranked.pos % 62)::integer + 1, 1)
                FROM ranked
                WHERE c."Id" = ranked."Id";
            """);

            migrationBuilder.Sql($"""
                WITH ranked AS (
                    SELECT "Id",
                           ROW_NUMBER() OVER (PARTITION BY "ColumnId" ORDER BY "Position"::integer) - 1 AS pos
                    FROM "Cards"
                )
                UPDATE "Cards" c
                SET "Position" = 'a' || substr('{DIGITS}', (ranked.pos % 62)::integer + 1, 1)
                FROM ranked
                WHERE c."Id" = ranked."Id";
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Columns" ALTER COLUMN "Position" TYPE integer
                USING CASE WHEN "Position" ~ '^[0-9]+$' THEN "Position"::integer ELSE 0 END;
            """);

            migrationBuilder.Sql("""
                ALTER TABLE "Cards" ALTER COLUMN "Position" TYPE integer
                USING CASE WHEN "Position" ~ '^[0-9]+$' THEN "Position"::integer ELSE 0 END;
            """);
        }
    }
}
