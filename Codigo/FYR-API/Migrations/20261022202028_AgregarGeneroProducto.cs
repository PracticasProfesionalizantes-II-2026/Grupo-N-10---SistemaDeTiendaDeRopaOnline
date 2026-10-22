using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FYR_API.Migrations
{
    /// <inheritdoc />
    public partial class AgregarGeneroProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Genero",
                table: "Productos",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Genero",
                table: "Productos");
        }
    }
}
