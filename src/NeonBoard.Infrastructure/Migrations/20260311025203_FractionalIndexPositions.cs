using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeonBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FractionalIndexPositions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Columns" ALTER COLUMN "Position" TYPE character varying(50)
                USING 'a' || "Position"::text;
            """);

            migrationBuilder.Sql("""
                ALTER TABLE "Cards" ALTER COLUMN "Position" TYPE character varying(50)
                USING 'a' || "Position"::text;
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
