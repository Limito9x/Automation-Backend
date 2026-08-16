using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Inspection.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "inspection");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:CollationDefinition:case_insensitive", "und-u-ks-level2,und-u-ks-level2,icu,False")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,");

            migrationBuilder.CreateTable(
                name: "Inspectors",
                schema: "inspection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExecutorKey = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inspectors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InspectorRules",
                schema: "inspection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlatformExtensionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    InspectorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectorRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectorRules_Inspectors_InspectorId",
                        column: x => x.InspectorId,
                        principalSchema: "inspection",
                        principalTable: "Inspectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InspectorVersions",
                schema: "inspection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntryPoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ScriptHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectorVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectorVersions_Inspectors_InspectorId",
                        column: x => x.InspectorId,
                        principalSchema: "inspection",
                        principalTable: "Inspectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Inspections",
                schema: "inspection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectorVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Data = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    ExecutionTimeMs = table.Column<long>(type: "bigint", nullable: false),
                    SummaryMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    InspectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inspections_InspectorVersions_InspectorVersionId",
                        column: x => x.InspectorVersionId,
                        principalSchema: "inspection",
                        principalTable: "InspectorVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_InspectorVersionId",
                schema: "inspection",
                table: "Inspections",
                column: "InspectorVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_ResourceVersionId",
                schema: "inspection",
                table: "Inspections",
                column: "ResourceVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_ResourceVersionId_InspectorVersionId",
                schema: "inspection",
                table: "Inspections",
                columns: new[] { "ResourceVersionId", "InspectorVersionId" });

            migrationBuilder.CreateIndex(
                name: "IX_InspectorRules_ContentTypeId",
                schema: "inspection",
                table: "InspectorRules",
                column: "ContentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectorRules_InspectorId",
                schema: "inspection",
                table: "InspectorRules",
                column: "InspectorId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectorRules_PlatformExtensionId",
                schema: "inspection",
                table: "InspectorRules",
                column: "PlatformExtensionId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectorRules_ProjectId",
                schema: "inspection",
                table: "InspectorRules",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectorVersions_InspectorId_Version",
                schema: "inspection",
                table: "InspectorVersions",
                columns: new[] { "InspectorId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inspectors_ProjectId",
                schema: "inspection",
                table: "Inspectors",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Inspectors_ProjectId_Key",
                schema: "inspection",
                table: "Inspectors",
                columns: new[] { "ProjectId", "Key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inspections",
                schema: "inspection");

            migrationBuilder.DropTable(
                name: "InspectorRules",
                schema: "inspection");

            migrationBuilder.DropTable(
                name: "InspectorVersions",
                schema: "inspection");

            migrationBuilder.DropTable(
                name: "Inspectors",
                schema: "inspection");
        }
    }
}
