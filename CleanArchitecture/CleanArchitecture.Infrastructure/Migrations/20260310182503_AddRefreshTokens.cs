using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshToken_users_ApplicationUserId",
                table: "RefreshToken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshToken",
                table: "RefreshToken");

            migrationBuilder.DropIndex(
                name: "IX_RefreshToken_ApplicationUserId",
                table: "RefreshToken");

            migrationBuilder.RenameTable(
                name: "RefreshToken",
                newName: "refresh_tokens");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Token",
                table: "refresh_tokens",
                newName: "token_hash");

            migrationBuilder.RenameColumn(
                name: "Revoked",
                table: "refresh_tokens",
                newName: "revoked_at");

            migrationBuilder.RenameColumn(
                name: "Expires",
                table: "refresh_tokens",
                newName: "expires_at");

            migrationBuilder.RenameColumn(
                name: "Created",
                table: "refresh_tokens",
                newName: "created_at");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "refresh_tokens",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "platform",
                table: "refresh_tokens",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_refresh_tokens",
                table: "refresh_tokens",
                columns: new[] { "ApplicationUserId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_refresh_tokens_users_ApplicationUserId",
                table: "refresh_tokens",
                column: "ApplicationUserId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_refresh_tokens_users_ApplicationUserId",
                table: "refresh_tokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_refresh_tokens",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "platform",
                table: "refresh_tokens");

            migrationBuilder.RenameTable(
                name: "refresh_tokens",
                newName: "RefreshToken");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "token_hash",
                table: "RefreshToken",
                newName: "Token");

            migrationBuilder.RenameColumn(
                name: "revoked_at",
                table: "RefreshToken",
                newName: "Revoked");

            migrationBuilder.RenameColumn(
                name: "expires_at",
                table: "RefreshToken",
                newName: "Expires");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "RefreshToken",
                newName: "Created");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "RefreshToken",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshToken",
                table: "RefreshToken",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_ApplicationUserId",
                table: "RefreshToken",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshToken_users_ApplicationUserId",
                table: "RefreshToken",
                column: "ApplicationUserId",
                principalTable: "users",
                principalColumn: "Id");
        }
    }
}
