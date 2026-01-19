# Script de setup para desenvolvimento do backend
# Execute este script na raiz do projeto backend

Write-Host "Configurando ambiente de desenvolvimento..." -ForegroundColor Green

# Verificar se .NET está instalado
$dotnetVersion = dotnet --version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ .NET SDK não encontrado. Por favor, instale o .NET 8.0 SDK." -ForegroundColor Red
    exit 1
}
Write-Host "✓ .NET SDK encontrado: $dotnetVersion" -ForegroundColor Green

# Restaurar pacotes NuGet
Write-Host "`nRestaurando pacotes NuGet..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Falha ao restaurar pacotes." -ForegroundColor Red
    exit 1
}

# Instalar ferramenta EF Core (se não estiver instalada)
Write-Host "`nVerificando ferramentas do Entity Framework..." -ForegroundColor Yellow
dotnet tool install --global dotnet-ef --version 8.0.0 2>&1 | Out-Null

# Criar migration inicial (se não existir)
$migrationPath = "src/SmartMeetingManager.Infrastructure/Migrations"
if (-not (Test-Path $migrationPath)) {
    Write-Host "`nCriando migration inicial..." -ForegroundColor Yellow
    dotnet ef migrations add InitialCreate `
        --project src/SmartMeetingManager.Infrastructure/SmartMeetingManager.Infrastructure.csproj `
        --startup-project src/SmartMeetingManager.API/SmartMeetingManager.API.csproj
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Falha ao criar migration." -ForegroundColor Red
        exit 1
    }
}

Write-Host "`n✅ Setup concluído com sucesso!" -ForegroundColor Green
Write-Host "`nProximos passos:" -ForegroundColor Cyan
Write-Host "   1. Certifique-se de que o PostgreSQL está rodando"
Write-Host "   2. Configure a connection string em appsettings.json"
Write-Host "   3. Execute: dotnet ef database update --project src/SmartMeetingManager.Infrastructure --startup-project src/SmartMeetingManager.API"
Write-Host "   4. Execute: dotnet run --project src/SmartMeetingManager.API"
