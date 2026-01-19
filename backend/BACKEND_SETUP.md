# Guia de Setup do Backend - Smart Meeting Manager

Este guia te ajudará a configurar e executar o backend da aplicação.

## Pré-requisitos

- **.NET 8.0 SDK** ou superior
- **PostgreSQL 14+** (ou usar Docker)
- **Visual Studio 2022** ou **VS Code** (opcional)

## Configuração Inicial

### 1. Restaurar Dependências

```bash
cd backend
dotnet restore
```

### 2. Configurar Banco de Dados

#### Opção A: PostgreSQL Local

Edite `src/SmartMeetingManager.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=SmartMeetingManager;Username=postgres;Password=sua_senha"
  }
}
```

#### Opção B: Docker (Recomendado)

```bash
# Na raiz do projeto, execute:
docker-compose up -d postgres
```

Isso irá criar um container PostgreSQL com:
- **Host**: localhost
- **Porta**: 5432
- **Database**: SmartMeetingManager
- **User**: postgres
- **Password**: postgres

### 3. Criar Migrations

```bash
# Instalar ferramenta EF (se ainda não tiver)
dotnet tool install --global dotnet-ef --version 8.0.0

# Criar migration inicial
cd src/SmartMeetingManager.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../SmartMeetingManager.API

# Aplicar migrations ao banco
dotnet ef database update --startup-project ../SmartMeetingManager.API
```

### 4. Executar a API

```bash
cd src/SmartMeetingManager.API
dotnet run
```

A API estará disponível em:
- **HTTP**: http://localhost:5000
- **HTTPS**: https://localhost:5001
- **Swagger UI**: https://localhost:5001/swagger

## Testando a API

### Endpoints Disponíveis

#### Health Check
```bash
GET http://localhost:5000/api/health
```

#### Listar Reuniões
```bash
GET http://localhost:5000/api/meetings
GET http://localhost:5000/api/meetings?organizationId=11111111-1111-1111-1111-111111111111
GET http://localhost:5000/api/meetings?projectId=44444444-4444-4444-4444-444444444444
```

#### Criar Reunião
```bash
POST http://localhost:5000/api/meetings
Content-Type: application/json

{
  "organizationId": "11111111-1111-1111-1111-111111111111",
  "projectId": "44444444-4444-4444-4444-444444444444",
  "title": "Reunião de Planejamento",
  "description": "Discussão sobre o próximo sprint",
  "type": "Planning",
  "scheduledAt": "2024-01-15T10:00:00Z",
  "duration": "01:00:00",
  "participantIds": ["33333333-3333-3333-3333-333333333333"]
}
```

#### Buscar Reunião por ID
```bash
GET http://localhost:5000/api/meetings/{id}
```

#### Atualizar Reunião
```bash
PUT http://localhost:5000/api/meetings/{id}
Content-Type: application/json

{
  "title": "Reunião Atualizada",
  "finalAgenda": "1. Ponto 1\n2. Ponto 2"
}
```

#### Gerar Pauta com IA
```bash
POST http://localhost:5000/api/meetings/{id}/generate-agenda
```

#### Processar Transcrição
```bash
POST http://localhost:5000/api/meetings/{id}/process-transcript
Content-Type: application/json

{
  "transcript": "Reunião iniciada... [texto da transcrição]"
}
```

## Dados de Teste

Ao iniciar a API em modo **Development**, dados de teste são criados automaticamente:

### Organização
- **ID**: `11111111-1111-1111-1111-111111111111`
- **Nome**: Empresa Teste

### Usuários
- **Admin User**
  - ID: `22222222-2222-2222-2222-222222222222`
  - Email: `admin@test.com`
- **Regular User**
  - ID: `33333333-3333-3333-3333-333333333333`
  - Email: `user@test.com`

### Projeto
- **ID**: `44444444-4444-4444-4444-444444444444`
- **Nome**: Projeto Exemplo

## Estrutura do Projeto

```
backend/
├── src/
│   ├── SmartMeetingManager.API/           # Controllers, Program.cs
│   ├── SmartMeetingManager.Application/    # Use Cases, DTOs, Mappings
│   ├── SmartMeetingManager.Domain/         # Entidades, Interfaces
│   └── SmartMeetingManager.Infrastructure/ # Repositórios, DbContext, Services
└── scripts/                                # Scripts auxiliares
```

## Troubleshooting

### Erro: "Could not connect to database"
- Verifique se o PostgreSQL está rodando
- Confirme a connection string em `appsettings.json`
- Teste a conexão: `psql -h localhost -U postgres -d SmartMeetingManager`

### Erro: "Migration not found"
- Execute: `dotnet ef migrations add InitialCreate --startup-project ../SmartMeetingManager.API`
- Depois: `dotnet ef database update --startup-project ../SmartMeetingManager.API`

### Porta já em uso
- Altere a porta no `appsettings.json` ou `launchSettings.json`
- Ou encerre o processo que está usando a porta

## Próximos Passos

- [ ] Implementar autenticação JWT
- [ ] Adicionar validações FluentValidation
- [ ] Implementar cache (Redis)
- [ ] Adicionar logging estruturado (Serilog)
- [ ] Implementar testes unitários
- [ ] Configurar CI/CD

---

Para mais informações, consulte o [README.md](../README.md) principal.
