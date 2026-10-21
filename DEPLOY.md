# Publicar o Milhas Gestor 100% grátis

O sistema são dois repositórios:

| Repositório | Conteúdo | Vai para |
|---|---|---|
| [`lucasmoroski/milhas-gestor-api`](https://github.com/lucasmoroski/milhas-gestor-api) | API .NET, Dockerfile, `render.yaml`, scripts, este guia | **Render** |
| [`lucasmoroski/milhas-gestor`](https://github.com/lucasmoroski/milhas-gestor) | Site React | **Vercel** |

Todos os serviços abaixo têm plano gratuito **sem prazo de expiração** e **sem cartão de crédito**:

| Peça | Serviço | Observações do plano gratuito |
|---|---|---|
| Banco de dados | **Neon** (PostgreSQL) | Não expira. Suspende após alguns minutos sem uso e acorda em ~1 s. |
| API (.NET) | **Render** (Web Service Docker) | Não expira. "Dorme" após ~15 min sem acesso; o primeiro acesso depois disso leva ~1 min. |
| Site (React) | **Vercel** | Não expira. Sem "sono". |

> Os limites exatos dos planos mudam com o tempo; confira nas páginas de preço antes de depender deles.
> O SQL Server continua sendo o banco do desenvolvimento local. Não existe SQL Server gratuito e permanente na nuvem,
> por isso o ambiente online usa PostgreSQL (a API suporta os dois: `Database__Provider`).

## O que depende só de você
Criar contas, aceitar termos e logar em Neon, Render e Vercel. São ~15 minutos de cliques; o resto já está no código.

**O link para usar em qualquer máquina é a URL que a Vercel mostrar ao final do passo 6** (algo como `https://milhas-gestor-xxxx.vercel.app`). Antes disso não existe link.

---

## 0. Proteja os dados reais
- `milhas_tech_data/` (nesta pasta, só local) tem dados pessoais e senhas de programas de milhas de clientes reais. Ela está no `.gitignore` e **não deve ir para o GitHub**.
- Mantenha os repositórios **privados**.
- LGPD: você é o controlador desses dados. O ambiente gratuito não tem SLA nem backup garantido.

## 1. Gerar os segredos (no seu computador)
```powershell
.\scripts\gerar-segredos.ps1
```
Guarde a **Crypto__Key** num gerenciador de senhas. Ela precisa ser igual no Render e na importação (passo 5).

## 2. Banco: Neon
1. Crie conta em https://neon.com e um projeto (região mais próxima, ex.: São Paulo, se disponível).
2. Em **Connection Details**, copie a connection string (`postgresql://usuario:senha@host/neondb?sslmode=require`). Ela é secreta.

## 3. Repositórios no GitHub
Já criados (privados). O envio é um `git push` em cada pasta, na conta `Lucasmoroskicurso@gmail.com`.

## 4. API: Render
1. Crie conta em https://render.com (pode entrar com o GitHub) e autorize o acesso ao repositório `milhas-gestor-api`.
2. **New → Blueprint** → escolha `milhas-gestor-api`. O Render lê o `render.yaml` e cria o serviço `milhas-api`.
3. Preencha as variáveis pedidas:
   - `ConnectionStrings__DefaultConnection` → string do Neon (passo 2)
   - `Crypto__Key` → a do passo 1
   - `Seed__AdminEmail` / `Seed__AdminPassword` → e-mail e uma senha **forte** do administrador
   - `Cors__AllowedOrigins__0` → provisório (`https://exemplo.vercel.app`); corrija no passo 6
4. Após o deploy, abra `https://SEU-SERVICO.onrender.com/health` — deve responder `{"status":"ok"}`.
   As tabelas são criadas automaticamente na primeira inicialização.

## 5. Importar os dados dos CSVs (no seu computador, nesta pasta)
```powershell
.\scripts\importar-dados-na-nuvem.ps1
```
O script pede a connection string e a Crypto__Key de forma oculta, mostra o servidor de destino e só prossegue se você digitar `SIM`.
Pode ser executado de novo sem duplicar (é idempotente).

> Os gestores dos CSVs entram com os e-mails e senhas que já tinham. As contas de "cliente" do sistema antigo
> **não** viram login (o portal do cliente não faz parte desta versão): seus dados aparecem apenas como clientes.

## 6. Site: Vercel
1. Crie conta em https://vercel.com (pode entrar com o GitHub) → **Add New → Project** → escolha `milhas-gestor`.
2. Framework: Vite (detectado). Root Directory: padrão (raiz).
3. Em **Environment Variables**, defina `VITE_API_URL` = `https://SEU-SERVICO.onrender.com/api` (com `/api` no final).
4. Deploy. Copie a URL final — **é o link do sistema**.
5. Volte ao Render → `Cors__AllowedOrigins__0` = essa URL (sem barra no final) → salve (o serviço reinicia).

Pronto: acesse a URL da Vercel de qualquer máquina e faça login.

---

## Operação
- **Primeiro acesso demorado:** se ninguém usou nos últimos ~15 min, a API leva cerca de 1 min para acordar. A tela de login avisa isso.
- **Reimportar/atualizar dados:** rode o script do passo 5 de novo.
- **Trocar a senha do admin:** pela tela Usuários (perfil Admin).
- **Backup:** no painel do Neon use *Branches/Restore* conforme o plano; para cópia local use `pg_dump` com a connection string.
- **Trabalhar em outra máquina:** `git clone` dos dois repositórios. Os CSVs não estão no Git; copie `milhas_tech_data/` à parte se precisar reimportar.
- **Alterar o modelo de dados:** crie uma migration para cada provider, na pasta `MilhasApi.Api` —
  ```powershell
  dotnet ef migrations add NomeDaMudanca --context MilhasDbContext -o Data/Migrations
  $env:Database__Provider="Postgres"; $env:ConnectionStrings__DefaultConnection="Host=localhost;Database=x;Username=x;Password=x"
  dotnet ef migrations add NomeDaMudancaPg --context MilhasDbContext -o Data/MigrationsPg
  ```
- **Plano B de hospedagem da API:** Koyeb, Fly.io e Azure têm camadas gratuitas, mas algumas exigem cartão. O `Dockerfile` serve para qualquer uma.

## Segurança já embutida
- A API **recusa iniciar** em produção sem `Jwt__Key` forte e `Crypto__Key`.
- Não há senha padrão: o admin só existe se você definir `Seed__*`.
- Senhas dos programas de milhas são gravadas com AES-256-GCM; só usuários autenticados as veem (e apenas dos próprios clientes, para o perfil Gestor).
- Login com limite de tentativas (10/min por IP), CORS restrito ao seu site, Swagger desligado em produção, HTTPS pela hospedagem.
