using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FYR_API.Migrations
{
    /// <inheritdoc />
    public partial class PersistirCarritoUsuarioV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF COL_LENGTH('dbo.Usuarios', 'CarritoJson') IS NULL ALTER TABLE dbo.Usuarios ADD CarritoJson nvarchar(max) NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF COL_LENGTH('dbo.Usuarios', 'CarritoJson') IS NOT NULL ALTER TABLE dbo.Usuarios DROP COLUMN CarritoJson;");
        }
    }
}
