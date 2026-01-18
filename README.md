# 🧠 Smart Meeting Manager

Sistema de Gestão de Reuniões Inteligente (Smart Meeting Manager) - Uma plataforma web que organiza, otimiza e extrai valor real das reuniões, usando IA para transformar informações dispersas em pautas objetivas, resumos acionáveis e tarefas atribuídas automaticamente.

## 🎯 Visão Geral

O Smart Meeting Manager não é apenas um agendador de reuniões. Ele reduz reuniões desnecessárias e garante a execução das decisões tomadas, transformando reuniões em ações concretas.

## 🏗️ Arquitetura

### Backend (.NET 8.0)
- **Arquitetura**: Clean Architecture / Hexagonal
- **Camadas**:
  - `SmartMeetingManager.Domain` - Entidades, interfaces e regras de negócio
  - `SmartMeetingManager.Application` - Casos de uso, DTOs e serviços de aplicação
  - `SmartMeetingManager.Infrastructure` - Acesso a dados, repositórios e integrações
  - `SmartMeetingManager.API` - Controllers, middleware e configurações

### Frontend (Next.js 14 + TypeScript)
- **Framework**: Next.js 14 com App Router
- **Estilização**: Tailwind CSS
- **Estado**: Zustand
- **Formulários**: React Hook Form + Zod
- **HTTP Client**: Axios

## 🚀 Funcionalidades

### 1. Gestão de Reuniões
- Criação manual ou automática via calendário
- Participantes, horário, objetivo e tipo de reunião
- Histórico completo por projeto/equipe

### 2. Sugestão Inteligente de Pauta (IA)
- Analisa e-mails recentes (Gmail / Outlook)
- Analisa tarefas pendentes (Jira, Trello, Azure DevOps - opcional)
- Gera pauta sugerida automaticamente

### 3. Gravação e Transcrição (Opcional)
- Upload de áudio/vídeo da reunião
- Speech-to-text
- Identificação de tópicos e decisões

### 4. Resumo Automático Pós-Reunião
- Resumo executivo
- Lista de decisões
- Ações com responsáveis e prazos

### 5. Atribuição Automática de Tarefas
- Criação automática de tarefas
- Integração com ferramentas externas (futuro)
- Status e acompanhamento

### 6. Dashboard de Produtividade
- Tempo gasto em reuniões por equipe
- Taxa de ações concluídas
- Reuniões com maior impacto
- Alertas de reuniões improdutivas

## 📋 Pré-requisitos

- .NET 8.0 SDK
- Node.js 18+ e npm/yarn
- PostgreSQL 14+
- (Opcional) Docker e Docker Compose

## 🔧 Configuração e Instalação

### Backend

1. Navegue até a pasta do backend:
```bash
cd backend
```

2. Configure a connection string no `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=SmartMeetingManager;Username=postgres;Password=postgres"
  }
}
```

3. Execute as migrations:
```bash
cd src/SmartMeetingManager.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../SmartMeetingManager.API
dotnet ef database update --startup-project ../SmartMeetingManager.API
```

4. Execute a API:
```bash
cd src/SmartMeetingManager.API
dotnet run
```

A API estará disponível em `http://localhost:5000` (ou porta configurada).

### Frontend

1. Navegue até a pasta do frontend:
```bash
cd frontend
```

2. Instale as dependências:
```bash
npm install
# ou
yarn install
```

3. Configure a URL da API no `.env.local`:
```env
NEXT_PUBLIC_API_URL=http://localhost:5000
```

4. Execute o servidor de desenvolvimento:
```bash
npm run dev
# ou
yarn dev
```

O frontend estará disponível em `http://localhost:3000`.

## 📁 Estrutura do Projeto

```
.
├── backend/
│   └── src/
│       ├── SmartMeetingManager.API/          # Camada de apresentação
│       ├── SmartMeetingManager.Application/   # Casos de uso e DTOs
│       ├── SmartMeetingManager.Domain/        # Entidades e interfaces
│       └── SmartMeetingManager.Infrastructure/# Repositórios e serviços
├── frontend/
│   └── src/
│       ├── app/                               # Páginas (Next.js App Router)
│       ├── components/                        # Componentes React
│       ├── services/                          # Serviços de API
│       ├── types/                             # Tipos TypeScript
│       └── lib/                               # Utilitários
└── README.md
```

## 🧪 Testes

### Backend
```bash
# Executar testes unitários
dotnet test

# Executar testes de integração
dotnet test --filter Category=Integration
```

### Frontend
```bash
# Executar testes
npm test
# ou
yarn test
```

## 🔐 Segurança

- Autenticação JWT + OAuth (Google/Microsoft)
- Controle de acesso por organização/equipe
- Criptografia de gravações
- Logs de auditoria
- LGPD-friendly (opt-in de gravação)

## 📈 Próximos Passos

- [ ] Implementar autenticação completa
- [ ] Integração com Google Calendar / Outlook
- [ ] Integração com Gmail / Microsoft Graph
- [ ] Implementar processamento de IA real (OpenAI/Azure OpenAI)
- [ ] Adicionar testes unitários e de integração
- [ ] Implementar jobs assíncronos (RabbitMQ/Kafka)
- [ ] Dashboard de produtividade
- [ ] Notificações em tempo real

## 📝 Licença

Este projeto está sob a licença MIT.

## 👥 Contribuindo

Contribuições são bem-vindas! Sinta-se à vontade para abrir issues e pull requests.

---

Desenvolvido com ❤️ para tornar reuniões mais produtivas e acionáveis.
