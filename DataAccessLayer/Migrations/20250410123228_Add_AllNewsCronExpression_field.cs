using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccessLayer.Migrations
{
    public partial class Add_AllNewsCronExpression_field : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AllNewsCronExpression",
                table: "ChatPermissions",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "RoleEntity",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "ConcurrencyStamp",
                value: "6cff67ab-5a1c-4479-94e5-b55c59cb0c10");

            migrationBuilder.UpdateData(
                table: "RoleEntity",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "ConcurrencyStamp",
                value: "7691f1c4-c47d-48d3-b8c3-314119dcc71f");

            migrationBuilder.UpdateData(
                table: "RoleEntity",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "ConcurrencyStamp",
                value: "a6f8fde3-144e-47f8-821c-82a39b49df7c");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllNewsCronExpression",
                table: "ChatPermissions");

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
    }
}
