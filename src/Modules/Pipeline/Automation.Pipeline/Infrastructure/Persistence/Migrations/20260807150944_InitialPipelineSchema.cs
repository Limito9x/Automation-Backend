using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Pipeline.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialPipelineSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "pipeline");

            migrationBuilder.CreateTable(
                name: "NodeDefinitions",
                schema: "pipeline",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RefId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_NodeDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PipelineItems",
                schema: "pipeline",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
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
                    table.PrimaryKey("PK_PipelineItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Scripts",
                schema: "pipeline",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    WorkerType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ScriptPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ParamsConfig = table.Column<JsonDocument>(type: "jsonb", nullable: false),
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
                    table.PrimaryKey("PK_Scripts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SessionDefinitions",
                schema: "pipeline",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    WorkerType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Flow = table.Column<JsonDocument>(type: "jsonb", nullable: false),
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
                    table.PrimaryKey("PK_SessionDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ToolDefinitions",
                schema: "pipeline",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    InputPins = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    OutputPins = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    HandlerKey = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
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
                    table.PrimaryKey("PK_ToolDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PipelineExecutions",
                schema: "pipeline",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PipelineId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FinishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_PipelineExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PipelineExecutions_PipelineItems_PipelineId",
                        column: x => x.PipelineId,
                        principalSchema: "pipeline",
                        principalTable: "PipelineItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PipelineNodes",
                schema: "pipeline",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PipelineId = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionX = table.Column<float>(type: "real", nullable: false),
                    PositionY = table.Column<float>(type: "real", nullable: false),
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
                    table.PrimaryKey("PK_PipelineNodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PipelineNodes_NodeDefinitions_NodeDefinitionId",
                        column: x => x.NodeDefinitionId,
                        principalSchema: "pipeline",
                        principalTable: "NodeDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PipelineNodes_PipelineItems_PipelineId",
                        column: x => x.PipelineId,
                        principalSchema: "pipeline",
                        principalTable: "PipelineItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NodeExecutions",
                schema: "pipeline",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PipelineExecutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PipelineNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Progress = table.Column<JsonDocument>(type: "jsonb", nullable: false),
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
                    table.PrimaryKey("PK_NodeExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NodeExecutions_PipelineExecutions_PipelineExecutionId",
                        column: x => x.PipelineExecutionId,
                        principalSchema: "pipeline",
                        principalTable: "PipelineExecutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NodeExecutions_PipelineNodes_PipelineNodeId",
                        column: x => x.PipelineNodeId,
                        principalSchema: "pipeline",
                        principalTable: "PipelineNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PipelineEdges",
                schema: "pipeline",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PipelineId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourcePipelineNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourcePin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TargetPipelineNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetPin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_PipelineEdges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PipelineEdges_PipelineItems_PipelineId",
                        column: x => x.PipelineId,
                        principalSchema: "pipeline",
                        principalTable: "PipelineItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PipelineEdges_PipelineNodes_SourcePipelineNodeId",
                        column: x => x.SourcePipelineNodeId,
                        principalSchema: "pipeline",
                        principalTable: "PipelineNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PipelineEdges_PipelineNodes_TargetPipelineNodeId",
                        column: x => x.TargetPipelineNodeId,
                        principalSchema: "pipeline",
                        principalTable: "PipelineNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NodeDefinitions_RefId",
                schema: "pipeline",
                table: "NodeDefinitions",
                column: "RefId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeExecutions_PipelineExecutionId",
                schema: "pipeline",
                table: "NodeExecutions",
                column: "PipelineExecutionId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeExecutions_PipelineNodeId",
                schema: "pipeline",
                table: "NodeExecutions",
                column: "PipelineNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_PipelineEdges_PipelineId",
                schema: "pipeline",
                table: "PipelineEdges",
                column: "PipelineId");

            migrationBuilder.CreateIndex(
                name: "IX_PipelineEdges_SourcePipelineNodeId",
                schema: "pipeline",
                table: "PipelineEdges",
                column: "SourcePipelineNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_PipelineEdges_TargetPipelineNodeId",
                schema: "pipeline",
                table: "PipelineEdges",
                column: "TargetPipelineNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_PipelineExecutions_PipelineId",
                schema: "pipeline",
                table: "PipelineExecutions",
                column: "PipelineId");

            migrationBuilder.CreateIndex(
                name: "IX_PipelineItems_ProjectId_Name",
                schema: "pipeline",
                table: "PipelineItems",
                columns: new[] { "ProjectId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PipelineNodes_NodeDefinitionId",
                schema: "pipeline",
                table: "PipelineNodes",
                column: "NodeDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_PipelineNodes_PipelineId",
                schema: "pipeline",
                table: "PipelineNodes",
                column: "PipelineId");

            migrationBuilder.CreateIndex(
                name: "IX_Scripts_Name",
                schema: "pipeline",
                table: "Scripts",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ToolDefinitions_Key",
                schema: "pipeline",
                table: "ToolDefinitions",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NodeExecutions",
                schema: "pipeline");

            migrationBuilder.DropTable(
                name: "PipelineEdges",
                schema: "pipeline");

            migrationBuilder.DropTable(
                name: "Scripts",
                schema: "pipeline");

            migrationBuilder.DropTable(
                name: "SessionDefinitions",
                schema: "pipeline");

            migrationBuilder.DropTable(
                name: "ToolDefinitions",
                schema: "pipeline");

            migrationBuilder.DropTable(
                name: "PipelineExecutions",
                schema: "pipeline");

            migrationBuilder.DropTable(
                name: "PipelineNodes",
                schema: "pipeline");

            migrationBuilder.DropTable(
                name: "NodeDefinitions",
                schema: "pipeline");

            migrationBuilder.DropTable(
                name: "PipelineItems",
                schema: "pipeline");
        }
    }
}
