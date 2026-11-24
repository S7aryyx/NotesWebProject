BEGIN;

CREATE SCHEMA IF NOT EXISTS "auth";
CREATE SCHEMA IF NOT EXISTS "content";
CREATE SCHEMA IF NOT EXISTS "billing";

DO $migration$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = 'Ilgam') THEN
        IF to_regclass('"Ilgam"."Users"') IS NOT NULL
           AND to_regclass('"auth"."Users"') IS NULL THEN
            ALTER TABLE "Ilgam"."Users" SET SCHEMA "auth";
        END IF;
        IF to_regclass('"Ilgam"."Folders"') IS NOT NULL
           AND to_regclass('"content"."Folders"') IS NULL THEN
            ALTER TABLE "Ilgam"."Folders" SET SCHEMA "content";
        END IF;
        IF to_regclass('"Ilgam"."Notes"') IS NOT NULL
           AND to_regclass('"content"."Notes"') IS NULL THEN
            ALTER TABLE "Ilgam"."Notes" SET SCHEMA "content";
        END IF;
        IF to_regclass('"Ilgam"."UserAccess"') IS NOT NULL
           AND to_regclass('"billing"."UserAccess"') IS NULL THEN
            ALTER TABLE "Ilgam"."UserAccess" SET SCHEMA "billing";
        END IF;
        IF to_regclass('"Ilgam"."PaymentTransactions"') IS NOT NULL
           AND to_regclass('"billing"."PaymentTransactions"') IS NULL THEN
            ALTER TABLE "Ilgam"."PaymentTransactions" SET SCHEMA "billing";
        END IF;
    END IF;
END
$migration$;

CREATE TABLE IF NOT EXISTS "auth"."Users"
(
    id UUID PRIMARY KEY,
    login VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL,
    password_hash TEXT NOT NULL,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT now(),
    CONSTRAINT uq_users_login UNIQUE (login),
    CONSTRAINT uq_users_email UNIQUE (email)
);

CREATE TABLE IF NOT EXISTS "content"."Folders"
(
    id UUID PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    parent_folder_id UUID NULL,
    owner_id UUID NOT NULL,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT now(),
    CONSTRAINT fk_folders_owner FOREIGN KEY (owner_id)
        REFERENCES "auth"."Users"(id) ON DELETE CASCADE,
    CONSTRAINT fk_folders_parent FOREIGN KEY (parent_folder_id)
        REFERENCES "content"."Folders"(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "content"."Notes"
(
    id UUID PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    content TEXT NOT NULL DEFAULT '',
    owner_id UUID NOT NULL,
    folder_id UUID NULL,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT now(),
    updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT now(),
    is_favorite BOOLEAN NOT NULL DEFAULT FALSE,
    is_archive BOOLEAN NOT NULL DEFAULT FALSE,
    timer TIMESTAMP WITHOUT TIME ZONE NULL,
    note_type VARCHAR(50) NULL,
    CONSTRAINT fk_notes_owner FOREIGN KEY (owner_id)
        REFERENCES "auth"."Users"(id) ON DELETE CASCADE,
    CONSTRAINT fk_notes_folder FOREIGN KEY (folder_id)
        REFERENCES "content"."Folders"(id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS "billing"."UserAccess"
(
    user_id UUID PRIMARY KEY REFERENCES "auth"."Users"(id) ON DELETE CASCADE,
    has_lifetime_access BOOLEAN NOT NULL DEFAULT FALSE,
    subscription_until TIMESTAMP WITH TIME ZONE NULL,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS "billing"."PaymentTransactions"
(
    id VARCHAR(100) PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES "auth"."Users"(id) ON DELETE CASCADE,
    plan VARCHAR(20) NOT NULL CHECK (plan IN ('monthly', 'lifetime')),
    amount NUMERIC(12,2) NOT NULL CHECK (amount > 0),
    currency CHAR(3) NOT NULL DEFAULT 'RUB',
    status VARCHAR(40) NOT NULL DEFAULT 'pending',
    paid BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT now(),
    updated_at TIMESTAMP WITH TIME ZONE NULL
);

CREATE INDEX IF NOT EXISTS ix_folders_owner ON "content"."Folders"(owner_id);
CREATE INDEX IF NOT EXISTS ix_folders_parent ON "content"."Folders"(parent_folder_id);
CREATE INDEX IF NOT EXISTS ix_notes_owner_updated ON "content"."Notes"(owner_id, updated_at DESC);
CREATE INDEX IF NOT EXISTS ix_notes_folder ON "content"."Notes"(folder_id);
CREATE INDEX IF NOT EXISTS ix_notes_archive_timer ON "content"."Notes"(is_archive, timer);
CREATE INDEX IF NOT EXISTS ix_payments_user_created ON "billing"."PaymentTransactions"(user_id, created_at DESC);

SELECT table_schema, table_name
FROM information_schema.tables
WHERE table_schema IN ('auth', 'content', 'billing')
  AND table_type = 'BASE TABLE'
ORDER BY table_schema, table_name;

COMMIT;
