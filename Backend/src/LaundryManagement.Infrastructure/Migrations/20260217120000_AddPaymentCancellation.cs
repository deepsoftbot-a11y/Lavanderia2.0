using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaundryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentCancellation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CanceladoEn",
                table: "Pagos",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CanceladoPor",
                table: "Pagos",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanceladoEn",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "CanceladoPor",
                table: "Pagos");
        }
    }
}
