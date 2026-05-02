using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SetValueDefaultZero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "value",
                table: "assets",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(12,2)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_events_title",
                table: "events",
                column: "title");

            migrationBuilder.CreateIndex(
                name: "IX_clubs_name",
                table: "clubs",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_budget_requests_title",
                table: "budget_requests",
                column: "title");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_Message",
                table: "Announcements",
                column: "Message");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_Title",
                table: "Announcements",
                column: "Title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_events_title",
                table: "events");

            migrationBuilder.DropIndex(
                name: "IX_clubs_name",
                table: "clubs");

            migrationBuilder.DropIndex(
                name: "IX_budget_requests_title",
                table: "budget_requests");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_Message",
                table: "Announcements");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_Title",
                table: "Announcements");

            migrationBuilder.AlterColumn<decimal>(
                name: "value",
                table: "assets",
                type: "numeric(12,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(12,2)");
        }
    }
}
