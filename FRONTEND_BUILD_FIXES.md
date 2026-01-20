# Correções de Build do Frontend

Este documento lista as correções aplicadas para garantir que o container do frontend suba corretamente.

## Problemas Identificados e Corrigidos

### 1. Dockerfile - Instalação de Dependências
**Problema**: O Dockerfile usava `npm ci` que requer `package-lock.json`, mas o arquivo pode não existir.

**Solução**: Alterado para `npm install --legacy-peer-deps` que funciona com ou sem `package-lock.json`.

### 2. Dockerfile - Ordem de Cópia de Arquivos
**Problema**: A ordem de cópia dos arquivos do build standalone estava incorreta.

**Solução**: Reordenado para copiar standalone primeiro, depois static e public.

### 3. TypeScript - Tipos de Status
**Problema**: `MeetingCard` usava objeto literal para `statusColors` sem tipagem adequada.

**Solução**: Adicionado tipo `Record<MeetingStatus, string>` e uso de enum para chaves.

### 4. Next.js Config - Standalone Output
**Problema**: Configuração do standalone pode não incluir todos os arquivos necessários.

**Solução**: Adicionado `experimental.outputFileTracingIncludes` para garantir que arquivos públicos sejam incluídos.

### 5. Diretório Public
**Problema**: Diretório `public` pode não existir, causando erro no build.

**Solução**: Criado `public/.gitkeep` para garantir que o diretório existe.

## Como Testar o Build

### Localmente (sem Docker)
```bash
cd frontend
npm install
npm run build
npm start
```

### Com Docker
```bash
# Build da imagem
docker build -t smm-frontend ./frontend

# Ou usando docker-compose
docker-compose build frontend
docker-compose up frontend
```

## Verificações Adicionais

Se o build ainda falhar, verifique:

1. **Erros de TypeScript**: Execute `npm run type-check`
2. **Erros de Lint**: Execute `npm run lint`
3. **Dependências**: Verifique se todas as dependências estão no `package.json`
4. **Variáveis de Ambiente**: Certifique-se de que `NEXT_PUBLIC_API_URL` está configurada

## Próximos Passos

- [ ] Adicionar testes de build no CI/CD
- [ ] Otimizar tamanho da imagem Docker
- [ ] Adicionar health check para o container
- [ ] Configurar cache de dependências no Docker
