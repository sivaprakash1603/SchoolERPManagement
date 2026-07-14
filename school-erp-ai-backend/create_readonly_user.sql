-- Create the readonly role if it doesn't exist
DO
$do$
BEGIN
   IF NOT EXISTS (
      SELECT FROM pg_catalog.pg_roles
      WHERE  rolname = 'ai_readonly_user') THEN

      CREATE ROLE ai_readonly_user WITH LOGIN PASSWORD 'secure_readonly_password123';
   END IF;
END
$do$;

-- Grant connect to database
GRANT CONNECT ON DATABASE "SchoolERPSystem" TO ai_readonly_user;

-- Grant usage on public schema
GRANT USAGE ON SCHEMA public TO ai_readonly_user;

-- Grant SELECT on all tables in public schema
GRANT SELECT ON ALL TABLES IN SCHEMA public TO ai_readonly_user;

-- Ensure future tables are also readable
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO ai_readonly_user;
