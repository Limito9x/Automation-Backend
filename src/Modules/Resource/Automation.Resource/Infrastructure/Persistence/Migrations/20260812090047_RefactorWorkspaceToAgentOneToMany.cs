using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Resource.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactorWorkspaceToAgentOneToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentWorkspaces",
                schema: "resource");

            migrationBuilder.AddColumn<Guid>(
                name: "AgentId",
                schema: "resource",
                table: "Workspaces",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RootPath",
                schema: "resource",
                table: "Workspaces",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_AgentId",
                schema: "resource",
                table: "Workspaces",
                column: "AgentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Workspaces_Agents_AgentId",
                schema: "resource",
                table: "Workspaces",
                column: "AgentId",
                principalSchema: "resource",
                principalTable: "Agents",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workspaces_Agents_AgentId",
                schema: "resource",
                table: "Workspaces");

            migrationBuilder.DropIndex(
                name: "IX_Workspaces_AgentId",
                schema: "resource",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "AgentId",
                schema: "resource",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "RootPath",
                schema: "resource",
                table: "Workspaces");

            migrationBuilder.CreateTable(
                name: "AgentWorkspaces",
                schema: "resource",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    LocalRootPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentWorkspaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentWorkspaces_Agents_AgentId",
                        column: x => x.AgentId,
                        principalSchema: "resource",
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AgentWorkspaces_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalSchema: "resource",
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentWorkspaces_AgentId_WorkspaceId",
                schema: "resource",
                table: "AgentWorkspaces",
                columns: new[] { "AgentId", "WorkspaceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgentWorkspaces_WorkspaceId",
                schema: "resource",
                table: "AgentWorkspaces",
                column: "WorkspaceId");
        }
    }
}
