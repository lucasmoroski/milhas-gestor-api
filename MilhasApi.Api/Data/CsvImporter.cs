using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using MilhasApi.Api.Models;
using MilhasApi.Api.Services;

namespace MilhasApi.Api.Data;

/// <summary>
/// Importa o export (CSV) do sistema anterior: usuarios, clientes, milhas, dependentes,
/// viagens, economia e notas. Idempotente: reexecutar atualiza em vez de duplicar (OrigemId).
/// Nao imprime dados pessoais no log; apenas contagens.
/// </summary>
public class CsvImporter
{
    private readonly MilhasDbContext _db;
    private readonly ISecretProtector _protector;
    private readonly TextWriter _log;
    private readonly string _dir;

    private sealed record UserRow(string Id, string Nome, string Email, string Hash, string Role);

    public CsvImporter(MilhasDbContext db, ISecretProtector protector, string dir, TextWriter log)
    {
        _db = db;
        _protector = protector;
        _dir = dir;
        _log = log;
    }

    public async Task RunAsync()
    {
        if (!Directory.Exists(_dir)) throw new DirectoryNotFoundException($"Pasta nao encontrada: {_dir}");

        var users = Read("users_rows.csv").Select(r => new UserRow(
            r.Req("id"), r.Str("name") ?? "", r.Str("email") ?? "", r.Str("password_hash") ?? "", (r.Str("role") ?? "").ToLowerInvariant()))
            .ToDictionary(u => u.Id);

        var gestorMap = await ImportGestoresAsync(users);
        var programas = await GarantirProgramasAsync();
        var clienteMap = await ImportClientesAsync(users, gestorMap);
        await ImportMilhasAsync(clienteMap, programas);
        await ImportDependentesAsync(clienteMap);
        var viagemMap = await ImportViagensAsync(clienteMap, programas);
        await ImportEconomiaAsync(clienteMap, viagemMap);
        await ImportNotasAsync(clienteMap, viagemMap, gestorMap);

        _log.WriteLine("Importacao concluida.");
    }

    // ---------- leitura ----------

    private List<Row> Read(string file)
    {
        var path = Path.Combine(_dir, file);
        if (!File.Exists(path))
        {
            _log.WriteLine($"[aviso] arquivo ausente: {file}");
            return new List<Row>();
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            BadDataFound = null,
            MissingFieldFound = null,
            DetectDelimiter = false
        };

        using var reader = new StreamReader(path, System.Text.Encoding.UTF8);
        using var csv = new CsvReader(reader, config);
        var rows = new List<Row>();
        if (!csv.Read()) return rows;
        csv.ReadHeader();
        var headers = csv.HeaderRecord ?? Array.Empty<string>();

        while (csv.Read())
        {
            var values = new Dictionary<string, object>();
            for (var i = 0; i < headers.Length; i++)
                values[headers[i].Trim()] = csv.GetField(i) ?? "";
            rows.Add(new Row(values));
        }
        return rows;
    }

    private sealed class Row
    {
        private readonly IDictionary<string, object> _values;
        public Row(IDictionary<string, object> values) => _values = values;

        public string? Str(string key)
        {
            if (!_values.TryGetValue(key, out var v) || v is null) return null;
            var s = v.ToString()?.Trim();
            return string.IsNullOrEmpty(s) ? null : s;
        }

        public string Req(string key) => Str(key) ?? throw new InvalidDataException($"Coluna obrigatoria vazia: {key}");

        public DateTime? Date(string key)
        {
            var s = Str(key);
            return s is not null && DateTime.TryParse(s, CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var d) ? d : null;
        }

        public decimal? Dec(string key)
        {
            var s = Str(key);
            return s is not null && decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : null;
        }

        public long? Long(string key) => Dec(key) is { } d ? (long)Math.Round(d) : null;

        public bool Bool(string key) => string.Equals(Str(key), "true", StringComparison.OrdinalIgnoreCase);
    }

    // ---------- helpers de upsert ----------

    private static async Task<Dictionary<string, T>> CarregarAsync<T>(IQueryable<T> query) where T : class, IComOrigem
    {
        var itens = await query.Where(x => x.OrigemId != null).ToListAsync();
        return itens.ToDictionary(x => x.OrigemId!);
    }

    private static string? Limpar(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    // ---------- etapas ----------

    private async Task<Dictionary<string, int>> ImportGestoresAsync(Dictionary<string, UserRow> users)
    {
        var existentes = await CarregarAsync(_db.Usuarios);
        var porEmail = await _db.Usuarios.ToDictionaryAsync(u => u.Email.ToLower());
        var map = new Dictionary<string, int>();
        int criados = 0, atualizados = 0, ignorados = 0;

        foreach (var u in users.Values.Where(u => u.Role == "gestor"))
        {
            var email = u.Email.Trim().ToLowerInvariant();
            if (email.Length == 0 || u.Hash.Length == 0) { ignorados++; continue; }

            if (!existentes.TryGetValue(u.Id, out var usuario))
            {
                if (porEmail.TryGetValue(email, out var jaExiste))
                {
                    // Conta com o mesmo e-mail ja existe (ex.: admin criado por configuracao): preserva.
                    map[u.Id] = jaExiste.Id;
                    ignorados++;
                    continue;
                }
                usuario = new Usuario { OrigemId = u.Id };
                _db.Usuarios.Add(usuario);
                criados++;
            }
            else atualizados++;

            usuario.Nome = u.Nome.Trim();
            usuario.Email = email;
            usuario.SenhaHash = u.Hash;
            usuario.Perfil = PerfilUsuario.Gestor;
            usuario.Ativo = true;
        }

        await _db.SaveChangesAsync();
        foreach (var kv in await _db.Usuarios.Where(x => x.OrigemId != null).Select(x => new { x.OrigemId, x.Id }).ToListAsync())
            map[kv.OrigemId!] = kv.Id;

        _log.WriteLine($"Gestores: {criados} criados, {atualizados} atualizados, {ignorados} ignorados (contas de clientes nao viram login).");
        return map;
    }

    private async Task<Dictionary<string, ProgramaFidelidade>> GarantirProgramasAsync()
    {
        var programas = await _db.ProgramasFidelidade.ToDictionaryAsync(p => p.Codigo);
        var usados = Read("client_miles_rows.csv").Select(r => r.Str("program_id"))
            .Concat(Read("trips_rows.csv").Select(r => r.Str("miles_program")))
            .Where(s => s is not null).Select(s => s!.ToLowerInvariant()).Distinct();

        foreach (var codigo in usados.Where(c => !programas.ContainsKey(c)))
        {
            var p = new ProgramaFidelidade { Codigo = codigo, Nome = codigo.ToUpperInvariant(), CompanhiaAerea = "" };
            _db.ProgramasFidelidade.Add(p);
            programas[codigo] = p;
            _log.WriteLine($"[aviso] programa '{codigo}' nao estava cadastrado; criado com o proprio codigo como nome.");
        }
        await _db.SaveChangesAsync();
        return programas;
    }

    private async Task<Dictionary<string, int>> ImportClientesAsync(Dictionary<string, UserRow> users, Dictionary<string, int> gestorMap)
    {
        var existentes = await CarregarAsync(_db.Clientes);
        int criados = 0, atualizados = 0, ignorados = 0;

        foreach (var r in Read("clients_rows.csv"))
        {
            var id = r.Req("id");
            if (!users.TryGetValue(r.Str("user_id") ?? "", out var user)) { ignorados++; continue; }

            if (!existentes.TryGetValue(id, out var c))
            {
                c = new Cliente { OrigemId = id };
                _db.Clientes.Add(c);
                existentes[id] = c;
                criados++;
            }
            else atualizados++;

            c.Nome = user.Nome.Trim();
            c.Email = Limpar(user.Email);
            c.Telefone = r.Str("phone");
            c.DataRenovacao = r.Date("renewal_date");
            c.ValorGestao = r.Dec("management_value");
            c.Observacoes = r.Str("notes");
            c.CriadoEm = r.Date("created_at") ?? DateTime.UtcNow;
            c.GestorId = r.Str("gestor_id") is { } g && gestorMap.TryGetValue(g, out var gid) ? gid : null;
        }

        await _db.SaveChangesAsync();
        _log.WriteLine($"Clientes: {criados} criados, {atualizados} atualizados, {ignorados} ignorados.");
        return existentes.ToDictionary(kv => kv.Key, kv => kv.Value.Id);
    }

    private async Task ImportMilhasAsync(Dictionary<string, int> clienteMap, Dictionary<string, ProgramaFidelidade> programas)
    {
        var existentes = await CarregarAsync(_db.ClientesMilhas);
        int criados = 0, atualizados = 0, ignorados = 0;

        foreach (var r in Read("client_miles_rows.csv"))
        {
            var id = r.Req("id");
            var codigo = r.Str("program_id")?.ToLowerInvariant();
            if (!clienteMap.TryGetValue(r.Str("client_id") ?? "", out var clienteId) || codigo is null || !programas.TryGetValue(codigo, out var programa))
            { ignorados++; continue; }

            if (!existentes.TryGetValue(id, out var m))
            {
                m = new ClienteMilhas { OrigemId = id };
                _db.ClientesMilhas.Add(m);
                criados++;
            }
            else atualizados++;

            m.ClienteId = clienteId;
            m.Programa = programa;
            m.Quantidade = r.Long("miles_balance") ?? 0;
            m.DataExpiracao = r.Date("expires_at");
            m.UltimaAtualizacao = r.Date("last_update") ?? DateTime.UtcNow;
            m.Login = r.Str("login");
            m.SenhaCriptografada = _protector.Protect(r.Str("password"));
        }

        await _db.SaveChangesAsync();
        _log.WriteLine($"Milhas por programa: {criados} criados, {atualizados} atualizados, {ignorados} ignorados.");
    }

    private async Task ImportDependentesAsync(Dictionary<string, int> clienteMap)
    {
        var existentes = await CarregarAsync(_db.Dependentes);
        int criados = 0, atualizados = 0, ignorados = 0;

        foreach (var r in Read("dependents_rows.csv"))
        {
            var id = r.Req("id");
            if (!clienteMap.TryGetValue(r.Str("client_id") ?? "", out var clienteId)) { ignorados++; continue; }

            if (!existentes.TryGetValue(id, out var d))
            {
                d = new Dependente { OrigemId = id };
                _db.Dependentes.Add(d);
                criados++;
            }
            else atualizados++;

            d.ClienteId = clienteId;
            d.Nome = r.Str("name") ?? "(sem nome)";
            d.DataNascimento = r.Date("birth_date");
            d.Parentesco = r.Str("relationship");
            d.CriadoEm = r.Date("created_at") ?? DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        _log.WriteLine($"Dependentes: {criados} criados, {atualizados} atualizados, {ignorados} ignorados.");
    }

    private async Task<Dictionary<string, int>> ImportViagensAsync(Dictionary<string, int> clienteMap, Dictionary<string, ProgramaFidelidade> programas)
    {
        var existentes = await CarregarAsync(_db.Viagens);
        int criados = 0, atualizados = 0, ignorados = 0;

        foreach (var r in Read("trips_rows.csv"))
        {
            var id = r.Req("id");
            if (!clienteMap.TryGetValue(r.Str("client_id") ?? "", out var clienteId)) { ignorados++; continue; }

            if (!existentes.TryGetValue(id, out var v))
            {
                v = new Viagem { OrigemId = id };
                _db.Viagens.Add(v);
                existentes[id] = v;
                criados++;
            }
            else atualizados++;

            var hotel = r.Str("hotel_ticket");
            var codigoPrograma = r.Str("miles_program")?.ToLowerInvariant();

            v.ClienteId = clienteId;
            v.Nome = r.Str("description") ?? "Viagem";
            v.Status = (r.Str("status")?.ToLowerInvariant()) switch
            {
                "emitida" => StatusViagem.Emitida,
                "confirmada" => StatusViagem.Confirmada,
                "finalizada" => StatusViagem.Finalizada,
                _ => StatusViagem.Planejada
            };
            v.Localizador = r.Str("locator");
            v.Companhia = r.Str("airline");
            v.Hotel = hotel is not null && hotel.Equals("sem hotel", StringComparison.OrdinalIgnoreCase) ? null : hotel;
            v.Origem = r.Str("origin");
            v.Destino = r.Str("destination");
            v.DataInicio = r.Date("start_date");
            v.DataFim = r.Date("end_date");
            v.SomenteIda = r.Bool("is_one_way");
            v.TipoPagamento = string.Equals(r.Str("payment_type"), "milhas", StringComparison.OrdinalIgnoreCase)
                ? TipoPagamentoViagem.Milhas : TipoPagamentoViagem.Pagante;
            v.Programa = codigoPrograma is not null && programas.TryGetValue(codigoPrograma, out var p) ? p : null;
            v.QuantidadeMilhas = r.Long("miles_amount");
            v.ValorPago = r.Dec("paid_amount");
            v.TaxaEmbarque = r.Dec("boarding_fee");
            v.Orcamento = r.Dec("budget");
            v.Observacoes = r.Str("notes");
            v.CriadoEm = r.Date("created_at") ?? DateTime.UtcNow;
            v.AtualizadoEm = r.Date("updated_at") ?? v.CriadoEm;
        }

        await _db.SaveChangesAsync();
        _log.WriteLine($"Viagens: {criados} criadas, {atualizados} atualizadas, {ignorados} ignoradas.");
        return existentes.ToDictionary(kv => kv.Key, kv => kv.Value.Id);
    }

    private async Task ImportEconomiaAsync(Dictionary<string, int> clienteMap, Dictionary<string, int> viagemMap)
    {
        var existentes = await CarregarAsync(_db.Economias);
        int criados = 0, atualizados = 0, ignorados = 0;

        foreach (var r in Read("economy_records_rows.csv"))
        {
            var id = r.Req("id");
            if (!clienteMap.TryGetValue(r.Str("client_id") ?? "", out var clienteId)) { ignorados++; continue; }

            if (!existentes.TryGetValue(id, out var e))
            {
                e = new EconomiaRegistro { OrigemId = id };
                _db.Economias.Add(e);
                criados++;
            }
            else atualizados++;

            e.ClienteId = clienteId;
            e.ViagemId = r.Str("trip_id") is { } t && viagemMap.TryGetValue(t, out var vid) ? vid : null;
            e.Valor = r.Dec("amount") ?? 0;
            e.Descricao = r.Str("description") ?? "Economia";
            e.CriadoEm = r.Date("created_at") ?? DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        _log.WriteLine($"Economia: {criados} criados, {atualizados} atualizados, {ignorados} ignorados.");
    }

    private async Task ImportNotasAsync(Dictionary<string, int> clienteMap, Dictionary<string, int> viagemMap, Dictionary<string, int> gestorMap)
    {
        var existentes = await CarregarAsync(_db.Notas);
        int criados = 0, atualizados = 0, ignorados = 0;

        foreach (var r in Read("notes_rows.csv"))
        {
            var id = r.Req("id");
            if (!clienteMap.TryGetValue(r.Str("client_id") ?? "", out var clienteId) || r.Str("content") is not { } conteudo)
            { ignorados++; continue; }

            if (!existentes.TryGetValue(id, out var n))
            {
                n = new Nota { OrigemId = id };
                _db.Notas.Add(n);
                criados++;
            }
            else atualizados++;

            n.ClienteId = clienteId;
            n.ViagemId = r.Str("trip_id") is { } t && viagemMap.TryGetValue(t, out var vid) ? vid : null;
            n.GestorId = r.Str("gestor_id") is { } g && gestorMap.TryGetValue(g, out var gid) ? gid : null;
            n.Conteudo = conteudo;
            n.VisivelAoCliente = r.Bool("visible_to_client");
            n.CriadoEm = r.Date("created_at") ?? DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        _log.WriteLine($"Notas: {criados} criadas, {atualizados} atualizadas, {ignorados} ignoradas.");
    }
}
