# Guia de Setup com Docker

Este guia explica como executar toda a aplicação usando Docker Compose.

## Pré-requisitos

- Docker 20.10+ instalado
- Docker Compose 2.0+ instalado

## Início Rápido

1. Clone o repositório (se ainda não tiver):
```bash
git clone <repository-url>
cd SMM
```

2. Execute o docker-compose:
```bash
docker-compose up -d
```

3. Aguarde os containers iniciarem (pode levar alguns minutos na primeira execução)

4. Acesse a aplicação:
- Frontend: http://localhost:3000
- Backend API: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger
- PostgreSQL: localhost:5432

## Estrutura dos Serviços

### PostgreSQL
- Container: `smm-postgres`
- Porta: 5432
- Database: SmartMeetingManager
- User: postgres
- Password: postgres

### Backend API
- Container: `smm-backend`
- Porta: 5000 (HTTP), 5001 (HTTPS)
- Health Check: http://localhost:5000/api/health

### Frontend
- Container: `smm-frontend`
- Porta: 3000
- Build: Next.js standalone mode

## Comandos Úteis

### Ver logs
```bash
# Todos os serviços
docker-compose logs -f

# Apenas backend
docker-compose logs -f backend

# Apenas frontend
docker-compose logs -f frontend

# Apenas postgres
docker-compose logs -f postgres
```

### Parar serviços
```bash
docker-compose down
```

### Parar e remover volumes (limpar dados)
```bash
docker-compose down -v
```

### Rebuild após mudanças
```bash
# Rebuild específico
docker-compose build backend
docker-compose build frontend

# Rebuild e restart
docker-compose up -d --build
```

### Executar comandos dentro dos containers

#### Backend
```bash
# Acessar shell do backend
docker-compose exec backend sh

# Executar migrations manualmente
docker-compose exec backend dotnet ef database update --project src/SmartMeetingManager.Infrastructure --startup-project src/SmartMeetingManager.API
```

#### Frontend
```bash
# Acessar shell do frontend
docker-compose exec frontend sh
```

#### PostgreSQL
```bash
# Acessar psql
docker-compose exec postgres psql -U postgres -d SmartMeetingManager

# Backup do banco
docker-compose exec postgres pg_dump -U postgres SmartMeetingManager > backup.sql

# Restore do banco
docker-compose exec -T postgres psql -U postgres SmartMeetingManager < backup.sql
```

## Variáveis de Ambiente

### Backend
- `ASPNETCORE_ENVIRONMENT`: Development/Production
- `ConnectionStrings__DefaultConnection`: Connection string do PostgreSQL
- `ASPNETCORE_URLS`: URLs que a API escuta

### Frontend
- `NODE_ENV`: production
- `NEXT_PUBLIC_API_URL`: URL da API backend

Para modificar, edite o arquivo `docker-compose.yml` ou crie um arquivo `.env`.

## Troubleshooting

### Erro: "Cannot connect to database"
- Verifique se o PostgreSQL está rodando: `docker-compose ps postgres`
- Verifique os logs: `docker-compose logs postgres`
- Aguarde o health check passar antes de iniciar o backend

### Erro: "Port already in use"
- Verifique se as portas 3000, 5000, 5001, 5432 estão livres
- Altere as portas no `docker-compose.yml` se necessário

### Backend não inicia
- Verifique os logs: `docker-compose logs backend`
- Verifique se o PostgreSQL está saudável: `docker-compose ps postgres`
- Tente rebuild: `docker-compose build backend`

### Frontend não conecta ao backend
- Verifique se `NEXT_PUBLIC_API_URL` está correto
- Em desenvolvimento local, use `http://localhost:5000`
- No Docker, use `http://backend:8080` (nome do serviço)

### Migrations não executam automaticamente
- Execute manualmente: `docker-compose exec backend dotnet ef database update --project src/SmartMeetingManager.Infrastructure --startup-project src/SmartMeetingManager.API`
- Ou acesse o container e execute os comandos

## Banco de Dados

### Dados Persistem?
Sim, os dados do PostgreSQL são salvos em um volume Docker chamado `postgres_data`. Mesmo removendo os containers, os dados permanecem.

### Limpar dados do banco
```bash
docker-compose down -v
```

Isso remove os containers E os volumes (dados serão perdidos).

### Backup manual
```bash
docker-compose exec postgres pg_dump -U postgres SmartMeetingManager > backup_$(date +%Y%m%d_%H%M%S).sql
```

## Estrutura dos Dockerfiles

### Backend Dockerfile
- Multi-stage build para otimizar tamanho
- Instala PostgreSQL client para health checks
- Instala EF Core tools para migrations
- Script de entrypoint aguarda PostgreSQL antes de iniciar

### Frontend Dockerfile
- Multi-stage build otimizado
- Usa Next.js standalone output
- Usuário não-root para segurança
- Build de produção otimizado

## Desenvolvimento

Para desenvolvimento com hot-reload, você pode:

1. Executar apenas o PostgreSQL no Docker:
```bash
docker-compose up -d postgres
```

2. Executar backend e frontend localmente:
```bash
# Backend
cd backend/src/SmartMeetingManager.API
dotnet run

# Frontend (outro terminal)
cd frontend
npm run dev
```

Isso permite mudanças serem refletidas imediatamente sem rebuild do Docker.

## Produção

Para produção, considere:

1. Usar variáveis de ambiente seguras (não hardcoded)
2. Configurar HTTPS adequadamente
3. Usar secrets do Docker ou variáveis de ambiente do host
4. Configurar backup automático do banco
5. Monitoramento e logging adequados
6. Health checks e restart policies já configurados

## Próximos Passos

- [ ] Adicionar nginx como reverse proxy
- [ ] Configurar SSL/TLS
- [ ] Adicionar Redis para cache
- [ ] Configurar CI/CD para builds automáticos
- [ ] Adicionar monitoring (Prometheus/Grafana)
