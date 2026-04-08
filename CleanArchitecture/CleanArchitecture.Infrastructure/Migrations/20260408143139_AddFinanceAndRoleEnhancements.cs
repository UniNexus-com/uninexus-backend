using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CleanArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFinanceAndRoleEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Created",
                table: "club_roles",
                newName: "created");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "club_roles",
                newName: "last_modified_by");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "club_roles",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "club_roles",
                newName: "created_by");

            migrationBuilder.AddColumn<decimal>(
                name: "total_budget",
                table: "clubs",
                type: "numeric(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "color",
                table: "club_roles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "club_roles",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "budget_requests",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, defaultValue: "PENDING"),
                    club_id = table.Column<int>(type: "integer", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_budget_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_budget_requests_clubs_club_id",
                        column: x => x.club_id,
                        principalTable: "clubs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_budget_requests_club_id",
                table: "budget_requests",
                column: "club_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "budget_requests");

            migrationBuilder.DropColumn(
                name: "total_budget",
                table: "clubs");

            migrationBuilder.DropColumn(
                name: "color",
                table: "club_roles");

            migrationBuilder.DropColumn(
                name: "description",
                table: "club_roles");

            migrationBuilder.RenameColumn(
                name: "created",
                table: "club_roles",
                newName: "Created");

            migrationBuilder.RenameColumn(
                name: "last_modified_by",
                table: "club_roles",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "club_roles",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "club_roles",
                newName: "CreatedBy");
        }
    }
}
