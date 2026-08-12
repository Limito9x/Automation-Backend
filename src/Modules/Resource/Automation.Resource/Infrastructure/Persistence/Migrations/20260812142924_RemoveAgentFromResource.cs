using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automation.Resource.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAgentFromResource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workspaces_Agents_AgentId",
                schema: "resource",
                table: "Workspaces");

            migrationBuilder.DropTable(
                name: "Agents",
                schema: "resource");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agents",
                schema: "resource",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    LastSeenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    MachineKey = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    RegistrationToken = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agents_MachineKey",
                schema: "resource",
                table: "Agents",
                column: "MachineKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Agents_RegistrationToken",
                schema: "resource",
                table: "Agents",
                column: "RegistrationToken",
                unique: true);

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
    }
}

