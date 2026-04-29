using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CleanArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClubChannels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "club_channels",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    club_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_club_channels", x => x.id);
                    table.ForeignKey(
                        name: "FK_club_channels_clubs_club_id",
                        column: x => x.club_id,
                        principalTable: "clubs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "club_channel_messages",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    channel_id = table.Column<int>(type: "integer", nullable: false),
                    sender_id = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_club_channel_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_club_channel_messages_club_channels_channel_id",
                        column: x => x.channel_id,
                        principalTable: "club_channels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_club_channel_messages_users_sender_id",
                        column: x => x.sender_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "club_channel_write_roles",
                columns: table => new
                {
                    channel_id = table.Column<int>(type: "integer", nullable: false),
                    club_role_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_club_channel_write_roles", x => new { x.channel_id, x.club_role_id });
                    table.ForeignKey(
                        name: "FK_club_channel_write_roles_club_channels_channel_id",
                        column: x => x.channel_id,
                        principalTable: "club_channels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_club_channel_write_roles_club_roles_club_role_id",
                        column: x => x.club_role_id,
                        principalTable: "club_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_club_channel_messages_channel_id",
                table: "club_channel_messages",
                column: "channel_id");

            migrationBuilder.CreateIndex(
                name: "IX_club_channel_messages_sender_id",
                table: "club_channel_messages",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "IX_club_channel_write_roles_club_role_id",
                table: "club_channel_write_roles",
                column: "club_role_id");

            migrationBuilder.CreateIndex(
                name: "IX_club_channels_club_id",
                table: "club_channels",
                column: "club_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "club_channel_messages");

            migrationBuilder.DropTable(
                name: "club_channel_write_roles");

            migrationBuilder.DropTable(
                name: "club_channels");
        }
    }
}
