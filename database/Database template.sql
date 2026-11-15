--Я создал БД notes_db и к ней применил данный запрос.

BEGIN;

CREATE SCHEMA IF NOT EXISTS "Ilgam";

CREATE TABLE IF NOT EXISTS "Ilgam"."Users"
(
    id UUID PRIMARY KEY,
    login VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL,
    password_hash TEXT NOT NULL,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT now(),
    CONSTRAINT uq_users_login UNIQUE (login),
    CONSTRAINT uq_users_email UNIQUE (email)
);

CREATE TABLE IF NOT EXISTS "Ilgam"."Folders"
(
    id UUID PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    parent_folder_id UUID NULL,
    owner_id UUID NOT NULL,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT now(),
    CONSTRAINT fk_folders_owner
        FOREIGN KEY (owner_id) REFERENCES "Ilgam"."Users"(id) ON DELETE CASCADE,
    CONSTRAINT fk_folders_parent
        FOREIGN KEY (parent_folder_id) REFERENCES "Ilgam"."Folders"(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "Ilgam"."Notes"
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
    CONSTRAINT fk_notes_owner
        FOREIGN KEY (owner_id) REFERENCES "Ilgam"."Users"(id) ON DELETE CASCADE,
    CONSTRAINT fk_notes_folder
        FOREIGN KEY (folder_id) REFERENCES "Ilgam"."Folders"(id) ON DELETE SET NULL
);

-- Entitlement: lifetime access takes precedence over monthly access.
CREATE TABLE IF NOT EXISTS "Ilgam"."UserAccess"
(
    user_id UUID PRIMARY KEY,
    has_lifetime_access BOOLEAN NOT NULL DEFAULT FALSE,
    subscription_until TIMESTAMP WITH TIME ZONE NULL,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT now(),
    CONSTRAINT fk_user_access_user
        FOREIGN KEY (user_id) REFERENCES "Ilgam"."Users"(id) ON DELETE CASCADE
);

-- One row per YooKassa payment. The payment ID is the provider's ID.
CREATE TABLE IF NOT EXISTS "Ilgam"."PaymentTransactions"
(
    id VARCHAR(100) PRIMARY KEY,
    user_id UUID NOT NULL,
    plan VARCHAR(20) NOT NULL CHECK (plan IN ('monthly', 'lifetime')),
    amount NUMERIC(12,2) NOT NULL CHECK (amount > 0),
    currency CHAR(3) NOT NULL DEFAULT 'RUB',
    status VARCHAR(40) NOT NULL DEFAULT 'pending',
    paid BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT now(),
    updated_at TIMESTAMP WITH TIME ZONE NULL,
    CONSTRAINT fk_payment_user
        FOREIGN KEY (user_id) REFERENCES "Ilgam"."Users"(id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_folders_owner
    ON "Ilgam"."Folders"(owner_id);

CREATE INDEX IF NOT EXISTS ix_folders_parent
    ON "Ilgam"."Folders"(parent_folder_id);

CREATE INDEX IF NOT EXISTS ix_notes_owner_updated
    ON "Ilgam"."Notes"(owner_id, updated_at DESC);

CREATE INDEX IF NOT EXISTS ix_notes_folder
    ON "Ilgam"."Notes"(folder_id);

CREATE INDEX IF NOT EXISTS ix_notes_archive_timer
    ON "Ilgam"."Notes"(is_archive, timer);

CREATE INDEX IF NOT EXISTS ix_payments_user_created
    ON "Ilgam"."PaymentTransactions"(user_id, created_at DESC);

COMMIT;

-- Quick verification:
-- SELECT table_schema, table_name
-- FROM information_schema.tables
-- WHERE table_schema = 'Ilgam'
-- ORDER BY table_name;
