using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccessLayer.Migrations
{
    public partial class TelegramUserEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TelegramPermissions_UserId",
                table: "TelegramPermissions");

            migrationBuilder.AddColumn<long>(
                name: "ChatId",
                table: "TelegramPermissions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.UpdateData(
                table: "RoleEntity",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "ConcurrencyStamp",
                value: "2df568b0-8760-4996-94f4-475fda9b849e");

            migrationBuilder.UpdateData(
                table: "RoleEntity",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "ConcurrencyStamp",
                value: "900b5373-2f43-41b9-bfe8-199bdd9342b9");

            migrationBuilder.UpdateData(
                table: "RoleEntity",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "ConcurrencyStamp",
                value: "2915c981-1a6b-4182-8c3a-58002c2a2058");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramPermissions_ChatId",
                table: "TelegramPermissions",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramPermissions_UserId_ChatId",
                table: "TelegramPermissions",
                columns: new[] { "UserId", "ChatId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TelegramPermissions_TelegramChanel_ChatId",
                table: "TelegramPermissions",
                column: "ChatId",
                principalTable: "TelegramChanel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TelegramPermissions_TelegramChanel_ChatId",
                table: "TelegramPermissions");

            migrationBuilder.DropIndex(
                name: "IX_TelegramPermissions_ChatId",
                table: "TelegramPermissions");

            migrationBuilder.DropIndex(
                name: "IX_TelegramPermissions_UserId_ChatId",
                table: "TelegramPermissions");

            migrationBuilder.DropColumn(
                name: "ChatId",
                table: "TelegramPermissions");

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
                name: "IX_TelegramPermissions_UserId",
                table: "TelegramPermissions",
                column: "UserId",
                unique: true);
        }
    }
}
