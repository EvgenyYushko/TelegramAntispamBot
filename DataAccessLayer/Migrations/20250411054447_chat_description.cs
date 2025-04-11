using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccessLayer.Migrations
{
    public partial class chat_description : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "TelegramChanel",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "RoleEntity",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "ConcurrencyStamp",
                value: "8978e42e-9bd5-49fc-945c-07b22f3ba547");

            migrationBuilder.UpdateData(
                table: "RoleEntity",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "ConcurrencyStamp",
                value: "384838c6-9ec7-495f-977c-6595dc265816");

            migrationBuilder.UpdateData(
                table: "RoleEntity",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "ConcurrencyStamp",
                value: "c83d72af-c778-40aa-8010-63501c5330e2");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "TelegramChanel");

            migrationBuilder.UpdateData(
                table: "RoleEntity",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "ConcurrencyStamp",
                value: "ba5ee8d6-3851-4cc9-9ecb-a7f578909e74");

            migrationBuilder.UpdateData(
                table: "RoleEntity",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "ConcurrencyStamp",
                value: "fb85b300-6f6a-4b98-82b8-8aa36eb595e6");

            migrationBuilder.UpdateData(
                table: "RoleEntity",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "ConcurrencyStamp",
                value: "7bc2a81a-5c43-4977-a8c7-314fb1406a50");
        }
    }
}
