# Erro ao rodar `dotnet ef database update`

Se aparecer:

```text
System.IO.FileNotFoundException: Could not load file or assembly 'System.Runtime, Version=10.0.0.0 ...'
```

é porque a ferramenta global `dotnet ef` está sendo executada com o runtime do .NET 10, enquanto o projeto usa .NET 8.

## Opção mais fácil: rodar a API em Development

Em **Development**, a API aplica migrações pendentes ao subir. Basta rodar o backend:

```bash
cd backend\src\SmartMeetingManager.API
dotnet run
```

Na primeira vez que conectar no banco, as migrações pendentes (incluindo `AddIsSiteAdmin`) serão aplicadas. Não é necessário usar `psql` nem `dotnet ef`.

---

## Opção 2: Aplicar a migração manualmente (PostgreSQL)

Se não quiser subir a API ou estiver em outro ambiente:

1. Conecte ao banco (psql, DBeaver, etc.) usando a connection string do `appsettings.json`.
2. Execute o script:

```sql
ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "IsSiteAdmin" boolean NOT NULL DEFAULT false;
```

Ou rode o arquivo:

```bash
psql -U seu_usuario -d sua_base -f backend/scripts/ApplyIsSiteAdminMigration.sql
```

Depois disso, o EF considera a migração aplicada. Para não tentar aplicá-la de novo, registre no banco:

```sql
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20240122000000_AddIsSiteAdmin', '8.0.0')
ON CONFLICT ("MigrationId") DO NOTHING;
```

**Se o `psql` não estiver no PATH:** use pgAdmin, DBeaver ou outro cliente: conecte no banco `SmartMeetingManager`, abra o arquivo `scripts/ApplyIsSiteAdminMigration.sql` e execute. Ou copie e cole o SQL acima em uma janela de consulta.

## Opção 3: Reinstalar a ferramenta `dotnet ef` (SDK 9)

1. Desinstale a ferramenta global:

   ```bash
   dotnet tool uninstall --global dotnet-ef
   ```

2. Force o uso do SDK 9 nesta pasta (o `backend/global.json` já faz isso).

3. Instale a ferramenta de novo (ela será associada ao SDK em uso):

   ```bash
   cd backend
   dotnet tool install --global dotnet-ef --version 8.0.0
   ```

4. Rode a migração:

   ```bash
   dotnet ef database update --project src/SmartMeetingManager.Infrastructure --startup-project src/SmartMeetingManager.API
   ```

**Importante:** sempre rode `dotnet ef` a partir da pasta `backend` (onde está o `global.json`), e use `--startup-project src/SmartMeetingManager.API`. Não execute a partir de `SmartMeetingManager.Infrastructure`, pois esse projeto é só uma biblioteca e não tem runtime.
