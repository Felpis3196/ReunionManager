#!/bin/bash

# Script de setup para desenvolvimento do backend
# Execute este script na raiz do projeto backend

echo "Configurando ambiente de desenvolvimento..."

# Verificar se .NET está instalado
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK não encontrado. Por favor, instale o .NET 8.0 SDK."
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo "✓ .NET SDK encontrado: $DOTNET_VERSION"

# Restaurar pacotes NuGet
echo ""
echo "Restaurando pacotes NuGet..."
dotnet restore
if [ $? -ne 0 ]; then
    echo "❌ Falha ao restaurar pacotes."
    exit 1
fi

# Instalar ferramenta EF Core (se não estiver instalada)
echo ""
echo "Verificando ferramentas do Entity Framework..."
dotnet tool install --global dotnet-ef --version 8.0.0 2>&1 > /dev/null

# Criar migration inicial (se não existir)
MIGRATION_PATH="src/SmartMeetingManager.Infrastructure/Migrations"
if [ ! -d "$MIGRATION_PATH" ]; then
    echo ""
    echo "Criando migration inicial..."
    dotnet ef migrations add InitialCreate \
        --project src/SmartMeetingManager.Infrastructure/SmartMeetingManager.Infrastructure.csproj \
        --startup-project src/SmartMeetingManager.API/SmartMeetingManager.API.csproj
    
    if [ $? -ne 0 ]; then
        echo "❌ Falha ao criar migration."
        exit 1
    fi
fi

echo ""
echo "✅ Setup concluído com sucesso!"
echo ""
echo "Proximos passos:"
echo "   1. Certifique-se de que o PostgreSQL está rodando"
echo "   2. Configure a connection string em appsettings.json"
echo "   3. Execute: dotnet ef database update --project src/SmartMeetingManager.Infrastructure --startup-project src/SmartMeetingManager.API"
echo "   4. Execute: dotnet run --project src/SmartMeetingManager.API"
