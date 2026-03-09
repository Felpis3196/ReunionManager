-- Migration: AddIsSiteAdmin (20240122000000)
-- Add IsSiteAdmin column to Users table (PostgreSQL).
-- Run this if "dotnet ef database update" fails with System.Runtime Version=10.0.0.0.

ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "IsSiteAdmin" boolean NOT NULL DEFAULT false;

-- Register migration so EF does not try to apply it again
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20240122000000_AddIsSiteAdmin', '8.0.0')
ON CONFLICT ("MigrationId") DO NOTHING;
