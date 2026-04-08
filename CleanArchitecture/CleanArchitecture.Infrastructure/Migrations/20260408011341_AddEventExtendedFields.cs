using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEventExtendedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "capacity",
                table: "events",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "events",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "require_approval",
                table: "events",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "requirements",
                table: "events",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tags",
                table: "events",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "visibility",
                table: "events",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                defaultValue: "All Students");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "capacity",
                table: "events");

            migrationBuilder.DropColumn(
                name: "category",
                table: "events");

            migrationBuilder.DropColumn(
                name: "require_approval",
                table: "events");

            migrationBuilder.DropColumn(
                name: "requirements",
                table: "events");

            migrationBuilder.DropColumn(
                name: "tags",
                table: "events");

            migrationBuilder.DropColumn(
                name: "visibility",
                table: "events");
        }
    }
}
