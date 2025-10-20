using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FrotaTaxi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CNPJ = table.Column<string>(type: "nvarchar(18)", maxLength: 18, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Logradouro = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Complemento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CEP = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Cidade = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Funcionalidades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Controller = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Funcionalidades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Perfis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perfis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Unidades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CPF = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    Apelido = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Celular = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Carro = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Placa = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    IMEI = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PercentualCorrida = table.Column<decimal>(type: "decimal(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unidades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CentrosCusto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CentrosCusto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CentrosCusto_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trechos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    NomeTrecho = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    TrechoInicio = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TrechoTermino = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trechos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trechos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosAutorizados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Funcional = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    TipoSolicitante = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Telefone1 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Telefone2 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosAutorizados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuariosAutorizados_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerfilFuncionalidades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PerfilId = table.Column<int>(type: "int", nullable: false),
                    FuncionalidadeId = table.Column<int>(type: "int", nullable: false),
                    PodeConsultar = table.Column<bool>(type: "bit", nullable: false),
                    PodeEditar = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilFuncionalidades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerfilFuncionalidades_Funcionalidades_FuncionalidadeId",
                        column: x => x.FuncionalidadeId,
                        principalTable: "Funcionalidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PerfilFuncionalidades_Perfis_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Login = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PerfilId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Perfis_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Corridas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    SolicitanteId = table.Column<int>(type: "int", nullable: false),
                    TipoTarifa = table.Column<int>(type: "int", nullable: false),
                    EnderecoInicial = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnderecoFinal = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    KmInicial = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    KmFinal = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    TrechoId = table.Column<int>(type: "int", nullable: true),
                    DataHoraAgendamento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UnidadeId = table.Column<int>(type: "int", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Observacao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CentroCustoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Corridas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Corridas_CentrosCusto_CentroCustoId",
                        column: x => x.CentroCustoId,
                        principalTable: "CentrosCusto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Corridas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Corridas_Trechos_TrechoId",
                        column: x => x.TrechoId,
                        principalTable: "Trechos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Corridas_Unidades_UnidadeId",
                        column: x => x.UnidadeId,
                        principalTable: "Unidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Corridas_UsuariosAutorizados_SolicitanteId",
                        column: x => x.SolicitanteId,
                        principalTable: "UsuariosAutorizados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Funcionalidades",
                columns: new[] { "Id", "Action", "Controller", "Nome" },
                values: new object[,]
                {
                    { 1, "Index", "Clientes", "Clientes" },
                    { 2, "Index", "Unidades", "Unidades" },
                    { 3, "Index", "Trechos", "Trechos" },
                    { 4, "Index", "Corridas", "Corridas" },
                    { 5, "Index", "Relatorios", "Relatórios" },
                    { 6, "Index", "Perfis", "Perfis" },
                    { 7, "Index", "Usuarios", "Usuários" }
                });

            migrationBuilder.InsertData(
                table: "Perfis",
                columns: new[] { "Id", "Nome" },
                values: new object[] { 1, "Administrador" });

            migrationBuilder.InsertData(
                table: "PerfilFuncionalidades",
                columns: new[] { "Id", "FuncionalidadeId", "PerfilId", "PodeConsultar", "PodeEditar" },
                values: new object[,]
                {
                    { 1, 1, 1, true, true },
                    { 2, 2, 1, true, true },
                    { 3, 3, 1, true, true },
                    { 4, 4, 1, true, true },
                    { 5, 5, 1, true, true },
                    { 6, 6, 1, true, true },
                    { 7, 7, 1, true, true }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Email", "Login", "Nome", "PerfilId" },
                values: new object[] { 1, "admin@frotataxi.com", "admin", "Administrador", 1 });

            migrationBuilder.CreateIndex(
                name: "IX_CentrosCusto_ClienteId",
                table: "CentrosCusto",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Corridas_CentroCustoId",
                table: "Corridas",
                column: "CentroCustoId");

            migrationBuilder.CreateIndex(
                name: "IX_Corridas_ClienteId",
                table: "Corridas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Corridas_SolicitanteId",
                table: "Corridas",
                column: "SolicitanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Corridas_TrechoId",
                table: "Corridas",
                column: "TrechoId");

            migrationBuilder.CreateIndex(
                name: "IX_Corridas_UnidadeId",
                table: "Corridas",
                column: "UnidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilFuncionalidades_FuncionalidadeId",
                table: "PerfilFuncionalidades",
                column: "FuncionalidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilFuncionalidades_PerfilId",
                table: "PerfilFuncionalidades",
                column: "PerfilId");

            migrationBuilder.CreateIndex(
                name: "IX_Trechos_ClienteId",
                table: "Trechos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_PerfilId",
                table: "Usuarios",
                column: "PerfilId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosAutorizados_ClienteId",
                table: "UsuariosAutorizados",
                column: "ClienteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Corridas");

            migrationBuilder.DropTable(
                name: "PerfilFuncionalidades");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "CentrosCusto");

            migrationBuilder.DropTable(
                name: "Trechos");

            migrationBuilder.DropTable(
                name: "Unidades");

            migrationBuilder.DropTable(
                name: "UsuariosAutorizados");

            migrationBuilder.DropTable(
                name: "Funcionalidades");

            migrationBuilder.DropTable(
                name: "Perfis");

            migrationBuilder.DropTable(
                name: "Clientes");
        }
    }
}
