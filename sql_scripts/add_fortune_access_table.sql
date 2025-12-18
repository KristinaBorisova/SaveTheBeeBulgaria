-- SQL script to add FortuneAccesses table
-- Run this directly on your SQLite database

CREATE TABLE IF NOT EXISTS FortuneAccesses (
    Id TEXT NOT NULL PRIMARY KEY,
    IpAddress TEXT NOT NULL,
    LastAccessDate TEXT NOT NULL,
    CreatedOn TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS IX_FortuneAccesses_IpAddress ON FortuneAccesses(IpAddress);

