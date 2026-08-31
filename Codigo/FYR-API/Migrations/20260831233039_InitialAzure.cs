using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FYR_API.Migrations
{
    /// <inheritdoc />
    public partial class InitialAzure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notificaciones_Notificaciones_NotificacionId",
                table: "Notificaciones");

            migrationBuilder.DropIndex(
                name: "IX_Notificaciones_NotificacionId",
                table: "Notificaciones");

            migrationBuilder.DropColumn(
                name: "NotificacionId",
                table: "Notificaciones");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NotificacionId",
                table: "Notificaciones",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_NotificacionId",
                table: "Notificaciones",
                column: "NotificacionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notificaciones_Notificaciones_NotificacionId",
                table: "Notificaciones",
                column: "NotificacionId",
                principalTable: "Notificaciones",
                principalColumn: "Id");
        }
    }
}
