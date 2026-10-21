using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MilhasApi.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProgramasFidelidade",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CompanhiaAerea = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CorHex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramasFidelidade", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SenhaHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Perfil = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrigemId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContasMilhas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramaFidelidadeId = table.Column<int>(type: "int", nullable: false),
                    NomeTitular = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CpfTitular = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LoginAcesso = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SaldoMilhas = table.Column<long>(type: "bigint", nullable: false),
                    CustoMedioMilheiro = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContasMilhas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContasMilhas_ProgramasFidelidade_ProgramaFidelidadeId",
                        column: x => x.ProgramaFidelidadeId,
                        principalTable: "ProgramasFidelidade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Cpf = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Telefone = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Observacoes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataRenovacao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValorGestao = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    GestorId = table.Column<int>(type: "int", nullable: true),
                    OrigemId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clientes_Usuarios_GestorId",
                        column: x => x.GestorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ComprasMilhas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContaMilhasId = table.Column<int>(type: "int", nullable: false),
                    FornecedorNome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    QuantidadeMilhas = table.Column<long>(type: "bigint", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    FormaPagamento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataCompra = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Observacoes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComprasMilhas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComprasMilhas_ContasMilhas_ContaMilhasId",
                        column: x => x.ContaMilhasId,
                        principalTable: "ContasMilhas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClientesMilhas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ProgramaFidelidadeId = table.Column<int>(type: "int", nullable: false),
                    Quantidade = table.Column<long>(type: "bigint", nullable: false),
                    DataExpiracao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UltimaAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Login = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SenhaCriptografada = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OrigemId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientesMilhas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientesMilhas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientesMilhas_ProgramasFidelidade_ProgramaFidelidadeId",
                        column: x => x.ProgramaFidelidadeId,
                        principalTable: "ProgramasFidelidade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Dependentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DataNascimento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Parentesco = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrigemId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dependentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dependentes_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tarefas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descricao = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    DataPrazo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Concluida = table.Column<bool>(type: "bit", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tarefas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tarefas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VendasEmissoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ContaMilhasId = table.Column<int>(type: "int", nullable: false),
                    CompanhiaAerea = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Localizador = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Origem = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Destino = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    DataViagem = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataEmissao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuantidadeMilhasUtilizadas = table.Column<long>(type: "bigint", nullable: false),
                    TaxaEmbarque = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ValorVenda = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    CustoMilhasNaVenda = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    ValorMercado = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    Hotel = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Observacoes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendasEmissoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendasEmissoes_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VendasEmissoes_ContasMilhas_ContaMilhasId",
                        column: x => x.ContaMilhasId,
                        principalTable: "ContasMilhas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Viagens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Localizador = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Companhia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Hotel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Origem = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Destino = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DataInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataFim = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SomenteIda = table.Column<bool>(type: "bit", nullable: false),
                    TipoPagamento = table.Column<int>(type: "int", nullable: false),
                    ProgramaFidelidadeId = table.Column<int>(type: "int", nullable: true),
                    QuantidadeMilhas = table.Column<long>(type: "bigint", nullable: true),
                    ValorPago = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    TaxaEmbarque = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    Orcamento = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    Observacoes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrigemId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Viagens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Viagens_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Viagens_ProgramasFidelidade_ProgramaFidelidadeId",
                        column: x => x.ProgramaFidelidadeId,
                        principalTable: "ProgramasFidelidade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LancamentosFinanceiros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    DataVencimento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    VendaEmissaoId = table.Column<int>(type: "int", nullable: true),
                    CompraMilhasId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LancamentosFinanceiros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LancamentosFinanceiros_ComprasMilhas_CompraMilhasId",
                        column: x => x.CompraMilhasId,
                        principalTable: "ComprasMilhas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LancamentosFinanceiros_VendasEmissoes_VendaEmissaoId",
                        column: x => x.VendaEmissaoId,
                        principalTable: "VendasEmissoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Economias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ViagemId = table.Column<int>(type: "int", nullable: true),
                    Valor = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrigemId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Economias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Economias_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Economias_Viagens_ViagemId",
                        column: x => x.ViagemId,
                        principalTable: "Viagens",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Notas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ViagemId = table.Column<int>(type: "int", nullable: true),
                    GestorId = table.Column<int>(type: "int", nullable: true),
                    Conteudo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VisivelAoCliente = table.Column<bool>(type: "bit", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrigemId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notas_Usuarios_GestorId",
                        column: x => x.GestorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notas_Viagens_ViagemId",
                        column: x => x.ViagemId,
                        principalTable: "Viagens",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_GestorId",
                table: "Clientes",
                column: "GestorId");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_OrigemId",
                table: "Clientes",
                column: "OrigemId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientesMilhas_ClienteId",
                table: "ClientesMilhas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientesMilhas_OrigemId",
                table: "ClientesMilhas",
                column: "OrigemId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientesMilhas_ProgramaFidelidadeId",
                table: "ClientesMilhas",
                column: "ProgramaFidelidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_ComprasMilhas_ContaMilhasId",
                table: "ComprasMilhas",
                column: "ContaMilhasId");

            migrationBuilder.CreateIndex(
                name: "IX_ContasMilhas_ProgramaFidelidadeId",
                table: "ContasMilhas",
                column: "ProgramaFidelidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_Dependentes_ClienteId",
                table: "Dependentes",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Dependentes_OrigemId",
                table: "Dependentes",
                column: "OrigemId");

            migrationBuilder.CreateIndex(
                name: "IX_Economias_ClienteId",
                table: "Economias",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Economias_OrigemId",
                table: "Economias",
                column: "OrigemId");

            migrationBuilder.CreateIndex(
                name: "IX_Economias_ViagemId",
                table: "Economias",
                column: "ViagemId");

            migrationBuilder.CreateIndex(
                name: "IX_LancamentosFinanceiros_CompraMilhasId",
                table: "LancamentosFinanceiros",
                column: "CompraMilhasId");

            migrationBuilder.CreateIndex(
                name: "IX_LancamentosFinanceiros_VendaEmissaoId",
                table: "LancamentosFinanceiros",
                column: "VendaEmissaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Notas_ClienteId",
                table: "Notas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Notas_GestorId",
                table: "Notas",
                column: "GestorId");

            migrationBuilder.CreateIndex(
                name: "IX_Notas_OrigemId",
                table: "Notas",
                column: "OrigemId");

            migrationBuilder.CreateIndex(
                name: "IX_Notas_ViagemId",
                table: "Notas",
                column: "ViagemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramasFidelidade_Codigo",
                table: "ProgramasFidelidade",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tarefas_ClienteId",
                table: "Tarefas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_OrigemId",
                table: "Usuarios",
                column: "OrigemId");

            migrationBuilder.CreateIndex(
                name: "IX_VendasEmissoes_ClienteId",
                table: "VendasEmissoes",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_VendasEmissoes_ContaMilhasId",
                table: "VendasEmissoes",
                column: "ContaMilhasId");

            migrationBuilder.CreateIndex(
                name: "IX_Viagens_ClienteId",
                table: "Viagens",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Viagens_OrigemId",
                table: "Viagens",
                column: "OrigemId");

            migrationBuilder.CreateIndex(
                name: "IX_Viagens_ProgramaFidelidadeId",
                table: "Viagens",
                column: "ProgramaFidelidadeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientesMilhas");

            migrationBuilder.DropTable(
                name: "Dependentes");

            migrationBuilder.DropTable(
                name: "Economias");

            migrationBuilder.DropTable(
                name: "LancamentosFinanceiros");

            migrationBuilder.DropTable(
                name: "Notas");

            migrationBuilder.DropTable(
                name: "Tarefas");

            migrationBuilder.DropTable(
                name: "ComprasMilhas");

            migrationBuilder.DropTable(
                name: "VendasEmissoes");

            migrationBuilder.DropTable(
                name: "Viagens");

            migrationBuilder.DropTable(
                name: "ContasMilhas");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "ProgramasFidelidade");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
