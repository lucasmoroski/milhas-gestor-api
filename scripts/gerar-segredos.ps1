# Gera segredos aleatórios para configurar a API na nuvem (Render).
# Nada é gravado em disco: copie os valores para o painel do Render e guarde Crypto__Key em um gerenciador de senhas.
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()

function Novo-Segredo([int]$bytes) {
    $b = New-Object byte[] $bytes
    $rng.GetBytes($b)
    [Convert]::ToBase64String($b)
}

Write-Host ""
Write-Host "Crypto__Key  (OBRIGATORIA e precisa ser a MESMA no Render e na importacao dos dados):" -ForegroundColor Cyan
Write-Host (Novo-Segredo 32)
Write-Host ""
Write-Host "Jwt__Key  (o Render ja gera sozinho com generateValue; use esta so se preferir definir manualmente):" -ForegroundColor Cyan
Write-Host (Novo-Segredo 48)
Write-Host ""
Write-Host "Atencao: se perder a Crypto__Key, as senhas dos programas de milhas ja gravadas ficam ilegiveis" -ForegroundColor Yellow
Write-Host "(basta reimportar os CSVs com uma nova chave para recria-las)."
