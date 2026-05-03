using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Enable pg_trgm extension for fast ILIKE '%x%' text search on indexed columns
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "clubs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_events_category",
                table: "events",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "IX_events_is_active",
                table: "events",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_events_start_date",
                table: "events",
                column: "start_date");

            migrationBuilder.CreateIndex(
                name: "IX_clubs_category",
                table: "clubs",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "IX_clubs_status",
                table: "clubs",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_budget_requests_created",
                table: "budget_requests",
                column: "created");

            migrationBuilder.CreateIndex(
                name: "IX_budget_requests_status",
                table: "budget_requests",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_budget_requests_status_created",
                table: "budget_requests",
                columns: new[] { "status", "created" });

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_ClubId",
                table: "Announcements",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_ClubId_Created",
                table: "Announcements",
                columns: new[] { "ClubId", "Created" });

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_Created",
                table: "Announcements",
                column: "Created");

            // GIN trigram indexes for fast ILIKE '%x%' text search
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_clubs_name_trgm ON clubs USING gin (name gin_trgm_ops);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_events_title_trgm ON events USING gin (title gin_trgm_ops);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_budget_requests_title_trgm ON budget_requests USING gin (title gin_trgm_ops);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_announcements_title_trgm ON \"Announcements\" USING gin (\"Title\" gin_trgm_ops);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_events_category",
                table: "events");

            migrationBuilder.DropIndex(
                name: "IX_events_is_active",
                table: "events");

            migrationBuilder.DropIndex(
                name: "IX_events_start_date",
                table: "events");

            migrationBuilder.DropIndex(
                name: "IX_clubs_category",
                table: "clubs");

            migrationBuilder.DropIndex(
                name: "IX_clubs_status",
                table: "clubs");

            migrationBuilder.DropIndex(
                name: "IX_budget_requests_created",
                table: "budget_requests");

            migrationBuilder.DropIndex(
                name: "IX_budget_requests_status",
                table: "budget_requests");

            migrationBuilder.DropIndex(
                name: "IX_budget_requests_status_created",
                table: "budget_requests");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_ClubId",
                table: "Announcements");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_ClubId_Created",
                table: "Announcements");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_Created",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "category",
                table: "clubs");

            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_clubs_name_trgm;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_events_title_trgm;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_budget_requests_title_trgm;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_announcements_title_trgm;");
        }
    }
}
