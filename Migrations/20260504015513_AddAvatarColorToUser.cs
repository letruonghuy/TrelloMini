using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JiraMini.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarColorToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarColor",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("0c5939f5-25a3-49fc-1731-08de5c89da4f"),
                column: "AvatarColor",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarColor",
                table: "Users");
        }
    }
}
