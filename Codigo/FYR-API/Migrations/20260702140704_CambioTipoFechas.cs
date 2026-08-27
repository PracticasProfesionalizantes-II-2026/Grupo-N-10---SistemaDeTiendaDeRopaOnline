using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FYR_API.Migrations
{
    /// <inheritdoc />
    public partial class CambioTipoFechas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FechaEstimada",
                table: "Envios",
                newName: "FechaEnvio");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaFactura",
                table: "Facturas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaFactura",
                table: "Facturas");

            migrationBuilder.RenameColumn(
                name: "FechaEnvio",
                table: "Envios",
                newName: "FechaEstimada");
        }
    }
}
