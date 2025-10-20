using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FrotaTaxi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCorridaDataTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Observacao",
                table: "Corridas",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "KmInicial",
                table: "Corridas",
                type: "decimal(10,2)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4)",
                oldMaxLength: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "KmFinal",
                table: "Corridas",
                type: "decimal(10,2)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4)",
                oldMaxLength: 4);

            migrationBuilder.AlterColumn<string>(
                name: "EnderecoInicial",
                table: "Corridas",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "EnderecoFinal",
                table: "Corridas",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Observacao",
                table: "Corridas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "KmInicial",
                table: "Corridas",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "KmFinal",
                table: "Corridas",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EnderecoInicial",
                table: "Corridas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EnderecoFinal",
                table: "Corridas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
