using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace DataAccessLayer.Migrations
{
    public partial class Chanels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TelegramChanel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: true),
                    ChatType = table.Column<string>(type: "text", nullable: true),
                    AddedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramChanel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TelegramChanel_TelegramUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "TelegramUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TelegramChannelAdmin",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ChannelId = table.Column<long>(type: "bigint", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramChannelAdmin", x => new { x.UserId, x.ChannelId });
                    table.ForeignKey(
                        name: "FK_TelegramChannelAdmin_TelegramChanel_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "TelegramChanel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TelegramChannelAdmin_TelegramUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "TelegramUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserChannelMembership",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ChannelId = table.Column<long>(type: "bigint", nullable: false),
                    JoinDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserChannelMembership", x => new { x.UserId, x.ChannelId });
                    table.ForeignKey(
                        name: "FK_UserChannelMembership_TelegramChanel_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "TelegramChanel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserChannelMembership_TelegramUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "TelegramUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "RoleEntity",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "ConcurrencyStamp",
                value: "489889d1-1be3-4138-b1c1-a3715af5ab00");

            migrationBuilder.UpdateData(
                table: "RoleEntity",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "ConcurrencyStamp",
                value: "248f1c11-773c-4340-83b5-b75fb5203a53");

            migrationBuilder.UpdateData(
                table: "RoleEntity",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "ConcurrencyStamp",
                value: "01060b30-7c52-49ef-b140-d0d7f5cc8bd2");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramChanel_CreatorId",
                table: "TelegramChanel",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramChannelAdmin_ChannelId",
                table: "TelegramChannelAdmin",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_UserChannelMembership_ChannelId",
                table: "UserChannelMembership",
                column: "ChannelId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TelegramChannelAdmin");

            migrationBuilder.DropTable(
                name: "UserChannelMembership");

            migrationBuilder.DropTable(
                name: "TelegramChanel");

            migrationBuilder.UpdateData(
                table: "RoleEntity",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "ConcurrencyStamp",
                value: "6c7a8a42-426e-41c2-b179-f5a554337014");

            migrationBuilder.UpdateData(
                table: "RoleEntity",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "ConcurrencyStamp",
                value: "9d4c5c65-8873-4e56-8fb5-3370e72fc8eb");

            migrationBuilder.UpdateData(
                table: "RoleEntity",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "ConcurrencyStamp",
                value: "43cb629c-0056-4382-8190-dbd700c06017");
        }
    }
}
