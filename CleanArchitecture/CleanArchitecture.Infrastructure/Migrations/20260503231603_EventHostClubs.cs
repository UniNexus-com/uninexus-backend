using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EventHostClubs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "event_clubs",
                columns: table => new
                {
                    event_id = table.Column<int>(type: "integer", nullable: false),
                    club_id = table.Column<int>(type: "integer", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_clubs", x => new { x.event_id, x.club_id });
                    table.ForeignKey(
                        name: "FK_event_clubs_clubs_club_id",
                        column: x => x.club_id,
                        principalTable: "clubs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_event_clubs_events_event_id",
                        column: x => x.event_id,
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                @"INSERT INTO event_clubs (event_id, club_id, sort_order)
                  SELECT id, club_id, 0 FROM events WHERE club_id IS NOT NULL;");

            migrationBuilder.DropForeignKey(
                name: "FK_events_clubs_club_id",
                table: "events");

            migrationBuilder.DropIndex(
                name: "IX_events_club_id",
                table: "events");

            migrationBuilder.DropColumn(
                name: "club_id",
                table: "events");

            migrationBuilder.CreateIndex(
                name: "IX_event_clubs_club_id",
                table: "event_clubs",
                column: "club_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "club_id",
                table: "events",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE events AS e
                  SET club_id = sub.club_id
                  FROM (
                    SELECT DISTINCT ON (event_id) event_id, club_id
                    FROM event_clubs
                    ORDER BY event_id, sort_order ASC
                  ) AS sub
                  WHERE e.id = sub.event_id;");

            migrationBuilder.DropTable(
                name: "event_clubs");

            migrationBuilder.CreateIndex(
                name: "IX_events_club_id",
                table: "events",
                column: "club_id");

            migrationBuilder.AddForeignKey(
                name: "FK_events_clubs_club_id",
                table: "events",
                column: "club_id",
                principalTable: "clubs",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
