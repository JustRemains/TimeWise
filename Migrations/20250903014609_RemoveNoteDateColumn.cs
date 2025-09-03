using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeWise.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNoteDateColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notes_Date",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "Notes");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_CreatedAt",
                table: "Notes",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_UpdatedAt",
                table: "Notes",
                column: "UpdatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notes_CreatedAt",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_UpdatedAt",
                table: "Notes");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Notes",
                type: "DATE",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Notes_Date",
                table: "Notes",
                column: "Date");
        }
    }
}
