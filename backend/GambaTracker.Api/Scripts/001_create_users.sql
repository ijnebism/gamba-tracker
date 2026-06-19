CREATE TABLE users (
    Id UUID PRIMARY KEY,
    Name TEXT NOT NULL,
    Email TEXT NOT NULL UNIQUE,
    PasswordHash TEXT,
    GoogleId TEXT
);