# Guia Completo de Teste - Smart Meeting Manager

## 🚀 Passo 1: Verificar se o Sistema Está Rodando

```powershell
docker ps
```

**Você deve ver 3 containers rodando:**
- `smm-postgres` - Status: Up (healthy)
- `smm-backend` - Status: Up (healthy)
- `smm-frontend` - Status: Up

**Se algum não estiver rodando:**
```powershell
docker compose up -d
```

## 📝 Passo 2: Testar Criação de Reunião pelo Frontend

### 2.1 Abrir o Navegador
1. Abra o Chrome/Edge/Firefox
2. Acesse: **http://localhost:3000**
3. **IMPORTANTE**: Abra o Console do Desenvolvedor (F12)

### 2.2 Criar uma Reunião de Teste

**Clique em "Nova Reunião" e preencha:**

```
Título: Reunião de Teste Sprint 15
Descrição: Esta é uma reunião de teste do sistema
Tipo: Planejamento
Duração: 01:30
Data e Hora: [Selecione AMANHÃ às 14:00]
Localização: Sala 3A
Link: https://meet.google.com/abc-def-ghi
```

### 2.3 Observar o Console

Quando você clicar em "Criar Reunião", no console você verá:

**Se der CERTO:**
```javascript
API Request: POST /api/meetings {dados...}
API Response: 201 {id, title, ...}
```

**Se der ERRO:**
```javascript
API Request: POST /api/meetings {dados...}
API Response Error: {
  status: 400,
  data: {
    error: "Erro de validação",
    message: "Data e hora devem ser no futuro",  // <-- MENSAGEM CLARA
    details: [...]
  }
}
Error submitting form: ...
```

## 🔍 Passo 3: Verificar Erros Comuns

### Erro 1: "Data e hora devem ser no futuro"
**Causa**: Você selecionou uma data/hora no passado
**Solução**: Selecione uma data FUTURA (amanhã ou depois)

### Erro 2: "Duração deve ser de pelo menos 1 minuto"
**Causa**: Duração está como "00:00"
**Solução**: Coloque pelo menos "00:01" ou "01:00"

### Erro 3: "Título é obrigatório"
**Causa**: Campo título está vazio
**Solução**: Preencha o título

### Erro 4: "URL deve ser válida"
**Causa**: URL do link está mal formatada
**Solução**: Use formato completo: `https://meet.google.com/abc-def-ghi`

### Erro 5: "Request failed with status code 400"
**Causa**: Erro genérico sem detalhes
**Solução**: Veja abaixo como debugar

## 🐛 Passo 4: Debug Detalhado

### 4.1 Ver Logs do Backend em Tempo Real

```powershell
docker logs -f smm-backend
```

**Deixe este comando rodando em um terminal**

### 4.2 Tentar Criar Reunião Novamente

No navegador, tente criar a reunião e observe o terminal com os logs.

**Você verá algo como:**
```
info: Microsoft.AspNetCore.Hosting.Internal.WebHost[1]
      Request starting HTTP/1.1 POST http://localhost:5000/api/meetings
Error creating meeting: System.ArgumentException: Data e hora devem ser no futuro
```

### 4.3 Copiar Payload da Requisição

No Console do Navegador, procure por:
```
API Request: POST /api/meetings {dados completos aqui}
```

**Copie esse JSON e me envie se precisar de ajuda**

## 🧪 Passo 5: Testar Diretamente via Swagger

### 5.1 Acessar Swagger
Abra: **http://localhost:5000/swagger**

### 5.2 Testar POST /api/meetings

1. Expanda **POST /api/meetings**
2. Clique em **"Try it out"**
3. Cole este JSON (ajuste a data para AMANHÃ):

```json
{
  "organizationId": "11111111-1111-1111-1111-111111111111",
  "title": "Teste via Swagger",
  "description": "Descrição teste",
  "type": "Planning",
  "scheduledAt": "2026-01-25T14:00:00",
  "duration": "01:30",
  "location": "Sala 3A",
  "meetingUrl": "https://meet.google.com/abc-def-ghi",
  "participantIds": []
}
```

4. Clique em **"Execute"**

**Resposta Esperada:**
- **Status 201** - Sucesso! ✅
- **Status 400** - Erro de validação com mensagem clara ⚠️

## 📊 Passo 6: Ver Reuniões Criadas

### Via Frontend
- Acesse: **http://localhost:3000**
- As reuniões aparecerão na home

### Via Swagger
1. Acesse: **http://localhost:5000/swagger**
2. Expanda **GET /api/meetings**
3. Clique em **"Try it out"**
4. Clique em **"Execute"**

### Via Logs do Backend
```powershell
docker logs smm-backend | Select-String "Creating meeting"
```

## 🔧 Troubleshooting Avançado

### Problema: Frontend não conecta ao Backend

**Verificar se o backend está rodando:**
```powershell
docker ps | Select-String "smm-backend"
```

**Testar health endpoint:**
Acesse no navegador: **http://localhost:5000/api/health**

**Deve retornar:**
```json
{
  "status": "healthy",
  "timestamp": "2026-01-20T...",
  "version": "1.0.0"
}
```

### Problema: Erro de CORS

Se ver erro tipo "blocked by CORS policy":

1. Verifique se está acessando via `localhost` (não `127.0.0.1`)
2. Verifique a URL da API no console

### Problema: Containers Param de Funcionar

**Reiniciar tudo:**
```powershell
docker compose down
docker compose up -d
```

**Aguardar 30 segundos e verificar:**
```powershell
docker ps
```

## 📸 Passo 7: Capturar Erro para Análise

Se ainda não funcionar, faça o seguinte:

### 7.1 No Navegador (F12 aberto)

1. Vá para aba **Network**
2. Tente criar a reunião
3. Procure pela linha que diz **"meetings"** (com status vermelho/400)
4. Clique nela
5. Vá para aba **Response**
6. **COPIE TODO O CONTEÚDO** e me envie

### 7.2 No Console

1. Vá para aba **Console**
2. Procure por linhas vermelhas com "Error"
3. **COPIE TUDO** e me envie

### 7.3 Logs do Backend

```powershell
docker logs smm-backend --tail 50 > backend-logs.txt
Get-Content backend-logs.txt
```

**COPIE o conteúdo** e me envie

## ✅ Checklist Rápido

Antes de reportar problema, verifique:

- [ ] Containers estão rodando (`docker ps`)
- [ ] Backend está healthy (`http://localhost:5000/api/health`)
- [ ] Console do navegador está aberto (F12)
- [ ] Data selecionada é FUTURA (não passado)
- [ ] Duração é maior que 00:00
- [ ] Título está preenchido
- [ ] URL do link está completa (com https://)
- [ ] Copiei o erro completo do console

## 🎯 Teste Mínimo Válido

**Use exatamente estes valores:**

```
Título: Teste
Tipo: Outro (Other)
Duração: 01:00
Data: 25/01/2026 14:00
```

**Deixe todos os outros campos vazios e tente criar.**

**Se funcionar:** ✅ Sistema OK!
**Se não funcionar:** 🐛 Me envie os logs conforme Passo 7

## 🆘 Links Rápidos

- Frontend: http://localhost:3000
- Backend Health: http://localhost:5000/api/health
- Swagger: http://localhost:5000/swagger
- Ver containers: `docker ps`
- Logs backend: `docker logs -f smm-backend`
- Reiniciar: `docker compose restart`
