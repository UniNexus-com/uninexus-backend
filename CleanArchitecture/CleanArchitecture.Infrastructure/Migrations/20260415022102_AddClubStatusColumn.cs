using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClubStatusColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "clubs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "ACTIVE");

            // Back-fill: inactive clubs become CLOSED, active clubs stay ACTIVE
            migrationBuilder.Sql(
                "UPDATE clubs SET status = CASE WHEN is_active = false THEN 'CLOSED' ELSE 'ACTIVE' END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "clubs");
        }
    }
}
