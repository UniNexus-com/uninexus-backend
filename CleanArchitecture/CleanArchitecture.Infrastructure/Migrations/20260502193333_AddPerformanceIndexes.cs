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
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

            // Veritabanı şema zaten güncellenmiş olabilir; idempotent DDL.
            migrationBuilder.Sql(@"ALTER TABLE clubs ADD COLUMN IF NOT EXISTS category character varying(50);");

            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_events_category"" ON events (category);");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_events_is_active"" ON events (is_active);");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_events_start_date"" ON events (start_date);");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_clubs_category"" ON clubs (category);");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_clubs_status"" ON clubs (status);");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_budget_requests_created"" ON budget_requests (created);");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_budget_requests_status"" ON budget_requests (status);");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_budget_requests_status_created"" ON budget_requests (status, created);");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_Announcements_ClubId"" ON ""Announcements"" (""ClubId"");");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_Announcements_ClubId_Created"" ON ""Announcements"" (""ClubId"", ""Created"");");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_Announcements_Created"" ON ""Announcements"" (""Created"");");

            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_clubs_name_trgm ON clubs USING gin (name gin_trgm_ops);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_events_title_trgm ON events USING gin (title gin_trgm_ops);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_budget_requests_title_trgm ON budget_requests USING gin (title gin_trgm_ops);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_announcements_title_trgm ON \"Announcements\" USING gin (\"Title\" gin_trgm_ops);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_events_category"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_events_is_active"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_events_start_date"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_clubs_category"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_clubs_status"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_budget_requests_created"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_budget_requests_status"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_budget_requests_status_created"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Announcements_ClubId"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Announcements_ClubId_Created"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Announcements_Created"";");

            migrationBuilder.Sql(@"ALTER TABLE clubs DROP COLUMN IF EXISTS category;");

            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_clubs_name_trgm;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_events_title_trgm;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_budget_requests_title_trgm;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_announcements_title_trgm;");
        }
    }
}
