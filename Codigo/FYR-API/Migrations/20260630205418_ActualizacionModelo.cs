using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FYR_API.Migrations
{
    /// <inheritdoc />
    public partial class ActualizacionModelo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetallesPedido_Productos_ProductoId",
                table: "DetallesPedido");

            migrationBuilder.DropForeignKey(
                name: "FK_Proveedores_Empresas_EmpresaId",
                table: "Proveedores");

            migrationBuilder.DropIndex(
                name: "IX_Sucursales_EmpresaId",
                table: "Sucursales");

            migrationBuilder.DropIndex(
                name: "IX_Subcategorias_CategoriaId",
                table: "Subcategorias");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_PedidoId",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_Envios_PedidoId",
                table: "Envios");

            migrationBuilder.DropIndex(
                name: "IX_Categorias_EmpresaId",
                table: "Categorias");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Envios",
                newName: "IdEnvio");

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Usuarios",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Usuarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "IdiomaPreferido",
                table: "Usuarios",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Usuarios",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Apellido",
                table: "Usuarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "FotoPerfil",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Sucursales",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Subcategorias",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "StockMinimo",
                table: "Stocks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "Proveedores",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "NotificacionId",
                table: "Notificaciones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Estado",
                table: "Envios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Cuit",
                table: "Empresas",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Categorias",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sucursales_EmpresaId_Nombre",
                table: "Sucursales",
                columns: new[] { "EmpresaId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subcategorias_CategoriaId_Nombre",
                table: "Subcategorias",
                columns: new[] { "CategoriaId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_NotificacionId",
                table: "Notificaciones",
                column: "NotificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_PedidoId",
                table: "Facturas",
                column: "PedidoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Envios_PedidoId",
                table: "Envios",
                column: "PedidoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_Cuit",
                table: "Empresas",
                column: "Cuit",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_EmpresaId_Nombre",
                table: "Categorias",
                columns: new[] { "EmpresaId", "Nombre" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesPedido_Productos_ProductoId",
                table: "DetallesPedido",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notificaciones_Notificaciones_NotificacionId",
                table: "Notificaciones",
                column: "NotificacionId",
                principalTable: "Notificaciones",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Proveedores_Empresas_EmpresaId",
                table: "Proveedores",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetallesPedido_Productos_ProductoId",
                table: "DetallesPedido");

            migrationBuilder.DropForeignKey(
                name: "FK_Notificaciones_Notificaciones_NotificacionId",
                table: "Notificaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Proveedores_Empresas_EmpresaId",
                table: "Proveedores");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Sucursales_EmpresaId_Nombre",
                table: "Sucursales");

            migrationBuilder.DropIndex(
                name: "IX_Subcategorias_CategoriaId_Nombre",
                table: "Subcategorias");

            migrationBuilder.DropIndex(
                name: "IX_Notificaciones_NotificacionId",
                table: "Notificaciones");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_PedidoId",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_Envios_PedidoId",
                table: "Envios");

            migrationBuilder.DropIndex(
                name: "IX_Empresas_Cuit",
                table: "Empresas");

            migrationBuilder.DropIndex(
                name: "IX_Categorias_EmpresaId_Nombre",
                table: "Categorias");

            migrationBuilder.DropColumn(
                name: "FotoPerfil",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "StockMinimo",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "NotificacionId",
                table: "Notificaciones");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Envios");

            migrationBuilder.RenameColumn(
                name: "IdEnvio",
                table: "Envios",
                newName: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "IdiomaPreferido",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Apellido",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Sucursales",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Subcategorias",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<int>(
                name: "EmpresaId",
                table: "Proveedores",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Cuit",
                table: "Empresas",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Categorias",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_Sucursales_EmpresaId",
                table: "Sucursales",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Subcategorias_CategoriaId",
                table: "Subcategorias",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_PedidoId",
                table: "Facturas",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Envios_PedidoId",
                table: "Envios",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_EmpresaId",
                table: "Categorias",
                column: "EmpresaId");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesPedido_Productos_ProductoId",
                table: "DetallesPedido",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Proveedores_Empresas_EmpresaId",
                table: "Proveedores",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id");
        }
    }
}
