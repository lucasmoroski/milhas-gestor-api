# Milhas API

Site (React): [`lucasmoroski/milhas-gestor`](https://github.com/lucasmoroski/milhas-gestor) · Publicação gratuita: [DEPLOY.md](DEPLOY.md)

## Rodando localmente
Pré-requisitos: .NET SDK 10 e SQL Server LocalDB (ou ajuste a connection string).
```powershell
cd MilhasApi.Api
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --urls http://localhost:5229          # Swagger em /swagger
dotnet run -- --import-csv "..\milhas_tech_data" # importa os CSVs (opcional, idempotente)
```
Em desenvolvimento a API cria o banco e um admin local definido em `appsettings.Development.json`. Fora do desenvolvimento **não existe senha padrão**.

API REST em ASP.NET Core (.NET 10) + Entity Framework Core. Suporta **SQL Server** (padrão, desenvolvimento) e **PostgreSQL** (`Database:Provider = Postgres`, usado na nuvem).

## Domínio
- **Clientes** (com gestor responsável, data de renovação e valor da gestão), **Milhas por programa** (login/senha criptografados), **Dependentes**, **Viagens**, **Economia** e **Notas**.
- **Programas de fidelidade** (Smiles, TudoAzul, Azul, LATAM Pass, Livelo, Esfera, TAP, Iberia, Caixa — criados automaticamente).
- **Dashboard** agregado por gestor.
- Módulo interno de estoque: Contas de milhas, Compras, Vendas/Emissões, Financeiro e Tarefas.

## Endpoints principais
| Recurso | Rotas |
|---|---|
| Auth | `POST /api/auth/login`, `GET /api/auth/me` |
| Dashboard | `GET /api/dashboard/resumo` |
| Clientes | `GET/POST /api/clientes`, `GET/PUT/DELETE /api/clientes/{id}` |
| Detalhe do cliente | `/api/clientes/{id}/milhas`, `/dependentes`, `/economia`, `/notas` (CRUD) |
| Viagens | `GET/POST /api/viagens`, `PUT /api/viagens/{id}`, `PUT /api/viagens/{id}/status`, `DELETE` |
| Programas | `GET/POST/PUT/DELETE /api/programas` |
| Saúde | `GET /health` |

## Configuração
| Chave | Descrição |
|---|---|
| `Database:Provider` | `SqlServer` (padrão) ou `Postgres` |
| `ConnectionStrings:DefaultConnection` | String do banco. Para Postgres aceita `postgresql://usuario:senha@host/banco?sslmode=require` |
| `Jwt:Key` | Segredo do token (**obrigatório forte fora de Development**) |
| `Crypto:Key` | Chave que criptografa as senhas dos programas (**obrigatória fora de Development**) |
| `Seed:AdminEmail` / `Seed:AdminPassword` | Cria o admin inicial se o e-mail não existir |
| `Cors:AllowedOrigins` | Origens do front permitidas |
| `Database:MigrateOnStartup` | `true` (padrão) aplica migrations ao iniciar |

Variáveis de ambiente usam `__` no lugar de `:` (ex.: `Jwt__Key`).

## Importar os CSVs
```powershell
dotnet run -- --import-csv "C:\caminho\milhas_tech_data"
```
Idempotente (usa `OrigemId`). Importa gestores (com o hash de senha original), clientes, milhas, dependentes, viagens, economia e notas. Contas de "cliente" do sistema antigo não viram login.

## Migrations (uma pasta por provider)
```powershell
dotnet ef migrations add Nome --context MilhasDbContext -o Data/Migrations
$env:Database__Provider="Postgres"; $env:ConnectionStrings__DefaultConnection="Host=localhost;Database=x;Username=x;Password=x"
dotnet ef migrations add NomePg --context MilhasDbContext -o Data/MigrationsPg
```

## Docker
```powershell
docker build -t milhas-api .
docker run -p 8080:8080 -e ConnectionStrings__DefaultConnection="..." -e Jwt__Key="..." -e Crypto__Key="..." -e Cors__AllowedOrigins__0="http://localhost:5173" milhas-api
```
