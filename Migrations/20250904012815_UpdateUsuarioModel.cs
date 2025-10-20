using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FrotaTaxi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUsuarioModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Login",
                table: "Usuarios",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Usuarios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCriacao",
                table: "Usuarios",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Senha",
                table: "Usuarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Ativo", "DataCriacao", "Senha" },
                values: new object[] { true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "DataCriacao",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Senha",
                table: "Usuarios");

            migrationBuilder.AlterColumn<string>(
                name: "Login",
                table: "Usuarios",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);
        }
    }
}
