# Como Ver Erros Detalhados - Sistema de Reuniões

## ✅ Sistema Está Rodando

O sistema agora tem **logging completo** em todas as camadas. Os erros agora são **100% claros e explícitos**.

## 🔍 Passo a Passo para Ver Erros

### 1. Abrir Console do Navegador

**SEMPRE abra o console ANTES de testar:**

1. Abra o Chrome/Edge/Firefox
2. Pressione **F12** (ou clique com botão direito > Inspecionar)
3. Vá para aba **Console**
4. Deixe aberto

### 2. Acessar a Aplicação

```
http://localhost:3000
```

### 3. Tentar Criar Reunião

Clique em "Nova Reunião" e preencha o formulário.

### 4. O Que Você Verá no Console

#### ✅ Quando DER CERTO:

```javascript
API Request: POST /api/meetings
{
  organizationId: "11111111-1111-1111-1111-111111111111",
  title: "Teste",
  type: "Other",
  scheduledAt: "2026-01-25T14:00:00",
  duration: "01:30",
  participantIds: []
}

API Response: 201
{
  id: "abc-123-...",
  title: "Teste",
  ... (dados da reunião criada)
}
```

#### ❌ Quando DER ERRO - Você Verá TUDO:

```javascript
API Request: POST /api/meetings
{... dados enviados ...}

API Response Error:
{
  status: 400,
  statusText: "Bad Request",
  data: {
    error: "Erro de validação",
    message: "Data e hora devem ser no futuro",  // ← MENSAGEM CLARA
    details: ["Data e hora devem ser no futuro"],
    type: "ArgumentException"
  },
  message: "Request failed with status code 400"
}

Error submitting form: AxiosError
Error response: {... detalhes completos ...}
```

### 5. O Que Você Verá na Tela

**Card de erro vermelho aparecerá mostrando:**

```
┌─────────────────────────────────────────┐
│ ⚠️ Erro ao criar reunião                │
│                                         │
│ Data e hora devem ser no futuro         │
│                                    [X]  │
└─────────────────────────────────────────┘
```

## 📋 Exemplos de Erros e Soluções

### Erro 1: "Data e hora devem ser no futuro"

**Mensagem Completa:**
```
Erro de validação
Data e hora devem ser no futuro
```

**Solução:**
- Selecione uma data FUTURA
- Exemplo: 25/01/2026 14:00 (amanhã ou depois)

---

### Erro 2: "Duração deve ser de pelo menos 1 minuto"

**Mensagem Completa:**
```
Erro de validação  
Duração deve ser de pelo menos 1 minuto
```

**Solução:**
- Coloque duração maior que 00:00
- Exemplo: 01:00 ou 01:30

---

### Erro 3: "Título é obrigatório"

**Mensagem Completa:**
```
Dados inválidos
Título é obrigatório
```

**Solução:**
- Preencha o campo Título
- Mínimo 1 caractere, máximo 200

---

### Erro 4: "URL deve ser válida"

**Mensagem Completa:**
```
Dados inválidos
URL deve ser válida
```

**Solução:**
- Use URL completa com https://
- Exemplo correto: `https://meet.google.com/abc-def-ghi`
- Exemplo errado: `meet.google.com` (falta https://)

---

### Erro 5: "Request failed with status code 400" (Genérico)

**Se ver apenas isso SEM detalhes:**

1. **Verifique o Console** - A mensagem completa está lá
2. **Copie TUDO do console** e me envie
3. **Verifique os logs do backend:**

```powershell
docker logs smm-backend --tail 30
```

## 🔧 Debug Avançado no Backend

### Ver Logs em Tempo Real:

```powershell
docker logs -f smm-backend
```

**Deixe este comando rodando e tente criar uma reunião.**

**Você verá:**

```
=== INÍCIO CREATE MEETING ===
Received DTO: OrganizationId=11111111-..., Title=Teste, Type=Other, ScheduledAt=2026-01-20T14:00:00, Duration=01:30
ModelState is valid. Proceeding to create meeting...
ArgumentException: Data e hora devem ser no futuro
```

---

## Usuário Site Admin default

- Na primeira vez que o backend sobe em desenvolvimento, o sistema garante a existência de um **usuário administrador da plataforma (Site Admin)**.
- Esse usuário é criado com:
  - **Email**: `admin@smm.local`
  - **Nome**: `Admin`
  - `IsSiteAdmin = true`
  - Uma organização global chamada **\"SmartMeeting Global\"**, onde ele entra como **Owner**.
- A senha inicial é definida assim:
  - Se existir `Seed:Admin:Password` na configuração (`appsettings.Development.json` ou variáveis de ambiente), esse valor é usado como senha.
  - Caso contrário, o backend gera uma **senha forte aleatória** e registra nos logs.
- Para descobrir a senha gerada:
  - Veja os logs do backend logo após o start:
    - Em desenvolvimento local: ao rodar `dotnet run`, procure por uma linha semelhante a  
      `Default SiteAdmin credentials - Email: admin@smm.local Password: <SENHA_AQUI>`
    - Em Docker: use algo como  
      `docker compose logs backend | findstr "Default SiteAdmin credentials"` (ou equivalente no seu shell).
- **Recomendado**:
  - Usar esse login apenas para configuração inicial.
  - Alterar a senha depois usando o fluxo de mudança de senha / esqueci minha senha.

## 🧪 Teste Garantido de Funcionar

Use **EXATAMENTE** estes valores:

```
Título: Teste Sistema
Descrição: (deixe vazio ou coloque qualquer texto)
Tipo: Outro
Duração: 01:00
Data e Hora: 25/01/2026 14:00 (IMPORTANTE: data FUTURA)
Localização: (deixe vazio)
Link: (deixe vazio)
```

**Clique em "Criar Reunião"**

### Se der certo:
- ✅ Toast verde aparece
- ✅ Você é redirecionado para home
- ✅ Reunião aparece na lista

### Se der erro:
- ❌ Card vermelho aparece com mensagem
- ❌ Console mostra detalhes completos
- ❌ Copie TUDO e me envie

## 📊 Verificar no Swagger

Se o frontend não funcionar, teste pelo Swagger:

1. Acesse: **http://localhost:5000/swagger**
2. Expanda **POST /api/meetings**
3. Clique em **"Try it out"**
4. Cole este JSON (ajuste a data para FUTURA):

```json
{
  "organizationId": "11111111-1111-1111-1111-111111111111",
  "title": "Teste Swagger",
  "type": "Other",
  "scheduledAt": "2026-01-25T14:00:00",
  "duration": "01:30",
  "participantIds": []
}
```

5. Clique em **"Execute"**

**Resposta esperada:**
- **Code 201** = ✅ SUCESSO!
- **Code 400** = ❌ ERRO com mensagem clara no Response body

## 🆘 Se Ainda Não Funcionar

Execute estes comandos e me envie a saída:

```powershell
# 1. Status dos containers
docker ps

# 2. Logs do backend
docker logs smm-backend --tail 50

# 3. Health check
Invoke-WebRequest -Uri 'http://localhost:5000/api/health' -UseBasicParsing | Select-Object -ExpandProperty Content

# 4. Testar criação direto
$body = @{
    organizationId='11111111-1111-1111-1111-111111111111'
    title='Teste PowerShell'
    type='Other'
    scheduledAt='2026-01-25T14:00:00'
    duration='01:30'
    participantIds=@()
} | ConvertTo-Json

try {
    Invoke-RestMethod -Uri 'http://localhost:5000/api/meetings' -Method POST -Body $body -ContentType 'application/json'
    Write-Host "SUCESSO!"
} catch {
    Write-Host "ERRO:"
    $_.Exception.Response.StatusCode
    $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
    $reader.BaseStream.Position = 0
    $reader.DiscardBufferedData()
    $responseBody = $reader.ReadToEnd()
    Write-Host $responseBody
}
```

## 📝 Resumo

**Agora o sistema tem:**
- ✅ Logging em TODAS as requests/responses
- ✅ Mensagens de erro CLARAS e em PORTUGUÊS
- ✅ Exibição visual dos erros na tela
- ✅ Detalhes completos no console do navegador
- ✅ Logs detalhados no backend

**Não há mais erro genérico!** Toda falha mostra exatamente o motivo.
