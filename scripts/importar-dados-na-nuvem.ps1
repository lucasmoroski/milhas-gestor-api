# Importa os CSVs de milhas_tech_data para o banco da NUVEM (Neon/PostgreSQL).
# Roda na sua máquina: a connection string e a chave de criptografia são pedidas de forma oculta
# e ficam apenas na memória deste processo (não vão para arquivo, histórico nem para o Git).
#
# Uso:  .\scripts\importar-dados-na-nuvem.ps1
#       .\scripts\importar-dados-na-nuvem.ps1 -Pasta "D:\outra\pasta\csv"
param(
    [string]$Pasta = (Join-Path $PSScriptRoot '..\milhas_tech_data')
)

$ErrorActionPreference = 'Stop'

function ConverterSecureString([System.Security.SecureString]$s) {
    (New-Object System.Net.NetworkCredential('', $s)).Password
}

$Pasta = (Resolve-Path $Pasta).Path
$projeto = (Resolve-Path (Join-Path $PSScriptRoot '..\MilhasApi.Api')).Path

Write-Host "Pasta dos CSVs : $Pasta"
$conn = ConverterSecureString (Read-Host 'Connection string do Neon (postgresql://...)' -AsSecureString)
$cryptoKey = ConverterSecureString (Read-Host 'Crypto__Key (a MESMA configurada no Render)' -AsSecureString)

if ($conn -notmatch '^postgres(ql)?://') { throw 'A connection string precisa comecar com postgresql://' }
if ($cryptoKey.Length -lt 16) { throw 'Crypto__Key precisa ter pelo menos 16 caracteres.' }

$uri = [Uri]$conn
Write-Host ""
Write-Host "Destino da importacao: servidor '$($uri.Host)', banco '$($uri.AbsolutePath.Trim('/'))'" -ForegroundColor Yellow
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
