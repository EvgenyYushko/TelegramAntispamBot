using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccessLayer.Migrations
{
    public partial class LinkUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserSiteId",
                table: "TelegramUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "RoleEntity",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "ConcurrencyStamp",
                value: "65b4177a-198d-46db-8703-abe922903176");

            migrationBuilder.UpdateData(
                table: "RoleEntity",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "ConcurrencyStamp",
                value: "7aafb8f0-70d3-4b80-868a-e34953d7bd3d");

            migrationBuilder.UpdateData(
                table: "RoleEntity",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "ConcurrencyStamp",
                value: "1022574d-f0ea-4688-805c-211dccfc09a7");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramUsers_UserSiteId",
                table: "TelegramUsers",
                column: "UserSiteId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TelegramUsers_UserEntity_UserSiteId",
                table: "TelegramUsers",
                column: "UserSiteId",
                principalTable: "UserEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TelegramUsers_UserEntity_UserSiteId",
                table: "TelegramUsers");

            migrationBuilder.DropIndex(
                name: "IX_TelegramUsers_UserSiteId",
                table: "TelegramUsers");

            migrationBuilder.DropColumn(
                name: "UserSiteId",
                table: "TelegramUsers");

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
        }
    }
}
