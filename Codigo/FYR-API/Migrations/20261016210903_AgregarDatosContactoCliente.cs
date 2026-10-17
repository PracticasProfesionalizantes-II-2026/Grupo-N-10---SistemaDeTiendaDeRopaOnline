using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FYR_API.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDatosContactoCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Domicilio",
                table: "Usuarios",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Localidad",
                table: "Usuarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Domicilio",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Localidad",
                table: "Usuarios");
        }
    }
}
