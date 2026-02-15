-- Add code field to projects table
-- The code is nullable to support gradual backfill
-- Unique constraint ensures no duplicate codes (NULLs don't conflict)

ALTER TABLE projects ADD COLUMN code TEXT;

-- Add unique constraint on code column
-- Note: NULL values don't violate unique constraints in PostgreSQL
CREATE UNIQUE INDEX projects_code_unique ON projects(code);
