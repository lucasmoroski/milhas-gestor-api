using Microsoft.EntityFrameworkCore;
using MilhasApi.Api.Models;

namespace MilhasApi.Api.Data;

public class MilhasDbContext : DbContext
{
    public MilhasDbContext(DbContextOptions<MilhasDbContext> options) : base(options) { }

    protected MilhasDbContext(DbContextOptions options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<ProgramaFidelidade> ProgramasFidelidade => Set<ProgramaFidelidade>();
    public DbSet<ContaMilhas> ContasMilhas => Set<ContaMilhas>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<CompraMilhas> ComprasMilhas => Set<CompraMilhas>();
    public DbSet<VendaEmissao> VendasEmissoes => Set<VendaEmissao>();
    public DbSet<LancamentoFinanceiro> LancamentosFinanceiros => Set<LancamentoFinanceiro>();
    public DbSet<Tarefa> Tarefas => Set<Tarefa>();
    public DbSet<Viagem> Viagens => Set<Viagem>();
    public DbSet<ClienteMilhas> ClientesMilhas => Set<ClienteMilhas>();
    public DbSet<Dependente> Dependentes => Set<Dependente>();
    public DbSet<EconomiaRegistro> Economias => Set<EconomiaRegistro>();
    public DbSet<Nota> Notas => Set<Nota>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.OrigemId);
            e.Property(u => u.Nome).HasMaxLength(150).IsRequired();
            e.Property(u => u.Email).HasMaxLength(200).IsRequired();
            e.Property(u => u.OrigemId).HasMaxLength(50);
        });

        modelBuilder.Entity<ProgramaFidelidade>(e =>
        {
            e.HasIndex(p => p.Codigo).IsUnique();
            e.Property(p => p.Codigo).HasMaxLength(30).IsRequired();
            e.Property(p => p.Nome).HasMaxLength(100).IsRequired();
            e.Property(p => p.CompanhiaAerea).HasMaxLength(100);
        });

        modelBuilder.Entity<ContaMilhas>(e =>
        {
            e.Property(c => c.NomeTitular).HasMaxLength(150).IsRequired();
            e.Property(c => c.CpfTitular).HasMaxLength(20).IsRequired();
            e.Property(c => c.CustoMedioMilheiro).HasColumnType("decimal(10,2)");
            e.HasOne(c => c.Programa)
                .WithMany(p => p.Contas)
                .HasForeignKey(c => c.ProgramaFidelidadeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cliente>(e =>
        {
            e.HasIndex(c => c.OrigemId);
            e.Property(c => c.Nome).HasMaxLength(150).IsRequired();
            e.Property(c => c.Email).HasMaxLength(200);
            e.Property(c => c.Telefone).HasMaxLength(40);
            e.Property(c => c.Cpf).HasMaxLength(20);
            e.Property(c => c.OrigemId).HasMaxLength(50);
            e.Property(c => c.ValorGestao).HasColumnType("decimal(12,2)");
            e.HasOne(c => c.Gestor)
                .WithMany()
                .HasForeignKey(c => c.GestorId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Tarefa>(e =>
        {
            e.Property(t => t.Descricao).HasMaxLength(300).IsRequired();
            e.HasOne(t => t.Cliente)
                .WithMany(c => c.Tarefas)
                .HasForeignKey(t => t.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Viagem>(e =>
        {
            e.HasIndex(v => v.OrigemId);
            e.Property(v => v.Nome).HasMaxLength(200).IsRequired();
            e.Property(v => v.Localizador).HasMaxLength(30);
            e.Property(v => v.Companhia).HasMaxLength(100);
            e.Property(v => v.Hotel).HasMaxLength(200);
            e.Property(v => v.Origem).HasMaxLength(200);
            e.Property(v => v.Destino).HasMaxLength(200);
            e.Property(v => v.OrigemId).HasMaxLength(50);
            e.Property(v => v.ValorPago).HasColumnType("decimal(12,2)");
            e.Property(v => v.TaxaEmbarque).HasColumnType("decimal(12,2)");
            e.Property(v => v.Orcamento).HasColumnType("decimal(12,2)");
            e.HasOne(v => v.Cliente)
                .WithMany(c => c.Viagens)
                .HasForeignKey(v => v.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(v => v.Programa)
                .WithMany()
                .HasForeignKey(v => v.ProgramaFidelidadeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ClienteMilhas>(e =>
        {
            e.HasIndex(m => m.OrigemId);
            e.Property(m => m.Login).HasMaxLength(200);
            e.Property(m => m.SenhaCriptografada).HasMaxLength(500);
            e.Property(m => m.OrigemId).HasMaxLength(50);
            e.HasOne(m => m.Cliente)
                .WithMany(c => c.Milhas)
                .HasForeignKey(m => m.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.Programa)
                .WithMany()
                .HasForeignKey(m => m.ProgramaFidelidadeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Dependente>(e =>
        {
            e.HasIndex(d => d.OrigemId);
            e.Property(d => d.Nome).HasMaxLength(150).IsRequired();
            e.Property(d => d.Parentesco).HasMaxLength(60);
            e.Property(d => d.OrigemId).HasMaxLength(50);
            e.HasOne(d => d.Cliente)
                .WithMany(c => c.Dependentes)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EconomiaRegistro>(e =>
        {
            e.HasIndex(x => x.OrigemId);
            e.Property(x => x.Descricao).HasMaxLength(300).IsRequired();
            e.Property(x => x.Valor).HasColumnType("decimal(12,2)");
            e.Property(x => x.OrigemId).HasMaxLength(50);
            e.HasOne(x => x.Cliente)
                .WithMany(c => c.Economias)
                .HasForeignKey(x => x.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Viagem)
                .WithMany()
                .HasForeignKey(x => x.ViagemId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Nota>(e =>
        {
            e.HasIndex(n => n.OrigemId);
            e.Property(n => n.Conteudo).IsRequired();
            e.Property(n => n.OrigemId).HasMaxLength(50);
            e.HasOne(n => n.Cliente)
                .WithMany(c => c.Notas)
                .HasForeignKey(n => n.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(n => n.Viagem)
                .WithMany()
                .HasForeignKey(n => n.ViagemId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(n => n.Gestor)
                .WithMany()
                .HasForeignKey(n => n.GestorId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CompraMilhas>(e =>
        {
            e.Property(c => c.FornecedorNome).HasMaxLength(150).IsRequired();
            e.Property(c => c.ValorTotal).HasColumnType("decimal(12,2)");
            e.Ignore(c => c.CustoPorMilheiro);
            e.HasOne(c => c.Conta)
                .WithMany(a => a.Compras)
                .HasForeignKey(c => c.ContaMilhasId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VendaEmissao>(e =>
        {
            e.Property(v => v.CompanhiaAerea).HasMaxLength(100).IsRequired();
            e.Property(v => v.Origem).HasMaxLength(60).IsRequired();
            e.Property(v => v.Destino).HasMaxLength(60).IsRequired();
            e.Property(v => v.Hotel).HasMaxLength(150);
            e.Property(v => v.TaxaEmbarque).HasColumnType("decimal(10,2)");
            e.Property(v => v.ValorVenda).HasColumnType("decimal(12,2)");
            e.Property(v => v.CustoMilhasNaVenda).HasColumnType("decimal(12,2)");
            e.Property(v => v.ValorMercado).HasColumnType("decimal(12,2)");
            e.Ignore(v => v.LucroEstimado);
            e.Ignore(v => v.Economia);

            e.HasOne(v => v.Cliente)
                .WithMany(c => c.Vendas)
                .HasForeignKey(v => v.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(v => v.Conta)
                .WithMany(a => a.Vendas)
                .HasForeignKey(v => v.ContaMilhasId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LancamentoFinanceiro>(e =>
        {
            e.Property(l => l.Categoria).HasMaxLength(100).IsRequired();
            e.Property(l => l.Descricao).HasMaxLength(300).IsRequired();
            e.Property(l => l.Valor).HasColumnType("decimal(12,2)");

            e.HasOne(l => l.VendaEmissao)
                .WithMany()
                .HasForeignKey(l => l.VendaEmissaoId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(l => l.CompraMilhas)
                .WithMany()
                .HasForeignKey(l => l.CompraMilhasId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}

/// <summary>Contexto usado quando Database:Provider = Postgres (migrations próprias em Data/MigrationsPg).</summary>
public class MilhasPgDbContext : MilhasDbContext
{
    public MilhasPgDbContext(DbContextOptions<MilhasPgDbContext> options) : base(options) { }
}
