using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClubSupporterSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SupporterCount",
                table: "ClubCreationRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "club_creation_request_supporters",
                columns: table => new
                {
                    club_creation_request_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    supported_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_club_creation_request_supporters", x => new { x.club_creation_request_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_club_creation_request_supporters_ClubCreationRequests_club_creation_request_id",
                        column: x => x.club_creation_request_id,
                        principalTable: "ClubCreationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "club_creation_request_supporters");

            migrationBuilder.DropColumn(
                name: "SupporterCount",
                table: "ClubCreationRequests");
        }
    }
}
