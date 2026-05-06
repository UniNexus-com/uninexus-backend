using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixStudentNumberNullableUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_StudentNumber",
                table: "users");

            migrationBuilder.RenameColumn(
                name: "Longitude",
                table: "events",
                newName: "longitude");

            migrationBuilder.RenameColumn(
                name: "Latitude",
                table: "events",
                newName: "latitude");

            migrationBuilder.CreateIndex(
                name: "IX_users_StudentNumber",
                table: "users",
                column: "StudentNumber",
                unique: true,
                filter: "\"StudentNumber\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_StudentNumber",
                table: "users");

            migrationBuilder.RenameColumn(
                name: "longitude",
                table: "events",
                newName: "Longitude");

            migrationBuilder.RenameColumn(
                name: "latitude",
                table: "events",
                newName: "Latitude");

            migrationBuilder.CreateIndex(
                name: "IX_users_StudentNumber",
                table: "users",
                column: "StudentNumber",
                unique: true);
        }
    }
}
