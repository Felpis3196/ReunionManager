# Script para limpar cache e fazer rebuild
Write-Host "Limpando cache do build..." -ForegroundColor Yellow

# Remove pastas bin e obj
Get-ChildItem -Path ".\src" -Recurse -Directory | Where-Object { $_.Name -eq "bin" -or $_.Name -eq "obj" } | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Cache limpo!" -ForegroundColor Green
Write-Host "Executando build..." -ForegroundColor Yellow

# Executa o build
dotnet build SmartMeetingManager.sln

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build concluido com sucesso!" -ForegroundColor Green
} else {
    Write-Host "Build falhou. Verifique os erros acima." -ForegroundColor Red
}
