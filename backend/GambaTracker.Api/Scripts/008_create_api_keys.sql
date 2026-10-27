-- Per-user keys for external sources (mods, scripts) posting to /api/ingest.
-- Only a SHA-256 hash of each key is stored; the full key is shown to the user once, when created.
CREATE TABLE api_keys (
    Id UUID PRIMARY KEY,
    UserId UUID NOT NULL REFERENCES users(Id) ON DELETE CASCADE,
    Name TEXT NOT NULL,
    KeyHash TEXT NOT NULL UNIQUE,
    Prefix TEXT NOT NULL,
    CreatedAt TIMESTAMPTZ NOT NULL,
    LastUsedAt TIMESTAMPTZ
);

CREATE INDEX ix_api_keys_user ON api_keys (UserId);
