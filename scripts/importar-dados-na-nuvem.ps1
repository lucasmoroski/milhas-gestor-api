# Importa os CSVs de milhas_tech_data para o banco da NUVEM (Neon/PostgreSQL).
# Roda na sua máquina: a connection string e a chave de criptografia ficam apenas na memória deste processo
# (não vão para arquivo, histórico nem para o Git).
#
# Uso (janela normal do PowerShell):
#   .\scripts\importar-dados-na-nuvem.ps1
#   .\scripts\importar-dados-na-nuvem.ps1 -Pasta "D:\outra\pasta\csv"
#
# Se o prompt oculto não aceitar a colagem, defina antes (vale só para esta janela) e rode o script:
#   $env:MILHAS_DB_URL = "postgresql://..."
#   $env:MILHAS_CRYPTO_KEY = "..."
param(
    [string]$Pasta = (Join-Path $PSScriptRoot '..\milhas_tech_data')
)

$ErrorActionPreference = 'Stop'

function ConverterSecureString([System.Security.SecureString]$s) {
    (New-Object System.Net.NetworkCredential('', $s)).Password
}

function LerSegredo([string]$variavel, [string]$pergunta) {
    $valor = [Environment]::GetEnvironmentVariable($variavel)
    if ([string]::IsNullOrWhiteSpace($valor)) {
        $valor = ConverterSecureString (Read-Host $pergunta -AsSecureString)
    }
    $valor
}

$Pasta = (Resolve-Path $Pasta).Path
$projeto = (Resolve-Path (Join-Path $PSScriptRoot '..\MilhasApi.Api')).Path

Write-Host "Pasta dos CSVs : $Pasta"
Write-Host "Prompt 1 = connection string do Neon (comeca com postgresql://). Prompt 2 = Crypto__Key." -ForegroundColor Cyan

$conn = LerSegredo 'MILHAS_DB_URL' '1) Connection string do Neon'
$cryptoKey = LerSegredo 'MILHAS_CRYPTO_KEY' '2) Crypto__Key (a MESMA configurada no Render)'

# Tolera espacos, aspas e o prefixo "psql '...'" que o painel do Neon oferece ao copiar.
$conn = ($conn.Trim() -replace '^psql\s+', '').Trim().Trim('"').Trim("'").Trim()
$cryptoKey = $cryptoKey.Trim().Trim('"').Trim("'")

if ($conn -notmatch '^postgres(ql)?://') {
    $inicio = if ($conn.Length -ge 4) { $conn.Substring(0, 4) } else { $conn }
    throw ("Valor do 1o prompt invalido: recebi {0} caracteres comecando com '{1}'. " -f $conn.Length, $inicio) +
          "Cole a connection string (postgresql://...) no 1o prompt e a Crypto__Key no 2o. " +
          "Se o prompt nao aceitar colagem, use as variaveis MILHAS_DB_URL e MILHAS_CRYPTO_KEY (veja o topo do script)."
}
if ($cryptoKey.Length -lt 16) { throw 'Crypto__Key precisa ter pelo menos 16 caracteres (2o prompt).' }

$uri = [Uri]$conn
$hostDestino = $uri.Host
if ($hostDestino -match '-pooler\.') {
    # Migrations precisam de conexao direta (o pooler PgBouncer do Neon nao suporta bem locks de sessao).
    $conn = $conn.Replace($hostDestino, ($hostDestino -replace '-pooler\.', '.'))
    $uri = [Uri]$conn
    $hostDestino = $uri.Host
    Write-Host "Aviso: usando o endpoint DIRETO do Neon (sem '-pooler') para a importacao." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Destino da importacao: servidor '$hostDestino', banco '$($uri.AbsolutePath.Trim('/'))'" -ForegroundColor Yellow
Write-Host "Isto grava dados reais de clientes (incluindo senhas de programas, criptografadas) nesse banco."
if ((Read-Host "Digite SIM para continuar") -ne 'SIM') { Write-Host 'Cancelado.'; return }

$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$b = New-Object byte[] 48; $rng.GetBytes($b)

$env:ASPNETCORE_ENVIRONMENT = 'Production'
$env:Database__Provider = 'Postgres'
$env:ConnectionStrings__DefaultConnection = $conn
$env:Crypto__Key = $cryptoKey
$env:Jwt__Key = [Convert]::ToBase64String($b)   # so para satisfazer a validacao de producao; nao e usado na importacao

try {
    dotnet run --project $projeto -c Release -- --import-csv $Pasta
}
finally {
    Remove-Item Env:ConnectionStrings__DefaultConnection, Env:Crypto__Key, Env:Jwt__Key -ErrorAction SilentlyContinue
}
