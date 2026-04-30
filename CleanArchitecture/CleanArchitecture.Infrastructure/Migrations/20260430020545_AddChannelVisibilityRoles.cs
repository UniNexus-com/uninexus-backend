using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChannelVisibilityRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "club_channel_visibility_roles",
                columns: table => new
                {
                    channel_id = table.Column<int>(type: "integer", nullable: false),
                    club_role_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_club_channel_visibility_roles", x => new { x.channel_id, x.club_role_id });
                    table.ForeignKey(
                        name: "FK_club_channel_visibility_roles_club_channels_channel_id",
                        column: x => x.channel_id,
                        principalTable: "club_channels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_club_channel_visibility_roles_club_roles_club_role_id",
                        column: x => x.club_role_id,
                        principalTable: "club_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_club_channel_visibility_roles_club_role_id",
                table: "club_channel_visibility_roles",
                column: "club_role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "club_channel_visibility_roles");
        }
    }
}
