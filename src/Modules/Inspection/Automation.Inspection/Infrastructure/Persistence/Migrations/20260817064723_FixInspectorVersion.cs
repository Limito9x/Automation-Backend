using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Inspection.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixInspectorVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE inspection.""InspectorVersions"" ALTER COLUMN ""Version"" TYPE integer USING (CASE WHEN ""Version"" ~ '^\d+$' THEN ""Version""::integer ELSE 1 END);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE inspection.""InspectorVersions"" ALTER COLUMN ""Version"" TYPE character varying(50) USING (""Version""::text);");
        }
    }
}
