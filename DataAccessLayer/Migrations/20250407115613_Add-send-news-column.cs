using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccessLayer.Migrations
{
    public partial class Addsendnewscolumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrencyCronExpression",
                table: "ChatPermissions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HabrCronExpression",
                table: "ChatPermissions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OnlinerCronExpression",
                table: "ChatPermissions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SendCurrency",
                table: "ChatPermissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SendHabr",
                table: "ChatPermissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SendOnliner",
                table: "ChatPermissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "RoleEntity",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "ConcurrencyStamp",
                value: "a813387f-1490-4cad-bd22-53aa4c3c91be");

            migrationBuilder.UpdateData(
                table: "RoleEntity",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "ConcurrencyStamp",
                value: "d59bebed-1f49-4200-9673-0b44fb1242ee");

            migrationBuilder.UpdateData(
                table: "RoleEntity",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "ConcurrencyStamp",
                value: "16fbb841-de62-4d93-badd-2bd531eab894");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrencyCronExpression",
                table: "ChatPermissions");

            migrationBuilder.DropColumn(
                name: "HabrCronExpression",
                table: "ChatPermissions");

            migrationBuilder.DropColumn(
                name: "OnlinerCronExpression",
                table: "ChatPermissions");

            migrationBuilder.DropColumn(
                name: "SendCurrency",
                table: "ChatPermissions");

            migrationBuilder.DropColumn(
                name: "SendHabr",
                table: "ChatPermissions");

            migrationBuilder.DropColumn(
                name: "SendOnliner",
                table: "ChatPermissions");

            migrationBuilder.UpdateData(
                table: "RoleEntity",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "ConcurrencyStamp",
                value: "3e70ad6e-214c-4b21-a9d5-ba58abfd254b");

            migrationBuilder.UpdateData(
                table: "RoleEntity",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "ConcurrencyStamp",
                value: "98b5755e-91cc-4aa8-a18c-709bc3857485");

            migrationBuilder.UpdateData(
                table: "RoleEntity",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "ConcurrencyStamp",
                value: "7b87b5f1-4aa0-407b-92b3-9734ab38b243");
        }
    }
}
