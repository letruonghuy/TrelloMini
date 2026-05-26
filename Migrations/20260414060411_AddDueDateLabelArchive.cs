using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrelloMini.Migrations
{
    /// <inheritdoc />
    public partial class AddDueDateLabelArchive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "TaskItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "TaskItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LabelColor",
                table: "TaskItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 14, 6, 4, 10, 178, DateTimeKind.Utc).AddTicks(1519));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("0c5939f5-25a3-49fc-1731-08de5c89da4f"),
                column: "CreatedAt",
                value: new DateTime(2026, 4, 14, 6, 4, 10, 178, DateTimeKind.Utc).AddTicks(1373));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "LabelColor",
                table: "TaskItems");

            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 11, 50, 10, 15, DateTimeKind.Utc).AddTicks(1053));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("0c5939f5-25a3-49fc-1731-08de5c89da4f"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 11, 50, 10, 15, DateTimeKind.Utc).AddTicks(920));
        }
    }
}
