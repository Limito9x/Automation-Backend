using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Inspection.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactorInspectionSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentTypeInspectorConfigs",
                schema: "inspection");

            migrationBuilder.DropTable(
                name: "InspectionItems",
                schema: "inspection");

            migrationBuilder.DropTable(
                name: "InspectionRecords",
                schema: "inspection");

            migrationBuilder.DropColumn(
                name: "PlatformKey",
                schema: "inspection",
                table: "Inspectors");

            migrationBuilder.DropColumn(
                name: "ScriptPath",
                schema: "inspection",
                table: "Inspectors");

            migrationBuilder.DropColumn(
                name: "SupportedExtension",
                schema: "inspection",
                table: "Inspectors");

            migrationBuilder.RenameColumn(
                name: "PrimaryFieldPath",
                schema: "inspection",
                table: "Inspectors",
                newName: "Name");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "inspection",
                table: "Inspectors",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

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
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectorVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    InspectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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
                name: "IX_Inspections_ResourceId",
                schema: "inspection",
                table: "Inspections",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_ResourceId_Version",
                schema: "inspection",
                table: "Inspections",
                columns: new[] { "ResourceId", "Version" },
                unique: true);

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

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "inspection",
                table: "Inspectors");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "inspection",
                table: "Inspectors",
                newName: "PrimaryFieldPath");

            migrationBuilder.AddColumn<string>(
                name: "PlatformKey",
                schema: "inspection",
                table: "Inspectors",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ScriptPath",
                schema: "inspection",
                table: "Inspectors",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SupportedExtension",
                schema: "inspection",
                table: "Inspectors",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ContentTypeInspectorConfigs",
                schema: "inspection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DisplayLabel = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    InspectorKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RelevantFieldPath = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentTypeInspectorConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InspectionRecords",
                schema: "inspection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    InspectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    InspectorKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    ResourceVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResultJson = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InspectionItems",
                schema: "inspection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    RawData = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionItems_InspectionRecords_InspectionId",
                        column: x => x.InspectionId,
                        principalSchema: "inspection",
                        principalTable: "InspectionRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentTypeInspectorConfigs_ContentTypeId_InspectorKey",
                schema: "inspection",
                table: "ContentTypeInspectorConfigs",
                columns: new[] { "ContentTypeId", "InspectorKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InspectionItems_InspectionId_Name",
                schema: "inspection",
                table: "InspectionItems",
                columns: new[] { "InspectionId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionRecords_ResourceVersionId",
                schema: "inspection",
                table: "InspectionRecords",
                column: "ResourceVersionId");
        }
    }
}

