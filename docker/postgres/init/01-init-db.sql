-- Initialize Rardi Database
\c rardi_db;

-- Create schemas for each service
CREATE SCHEMA IF NOT EXISTS security;
CREATE SCHEMA IF NOT EXISTS customer;
CREATE SCHEMA IF NOT EXISTS inventory;
CREATE SCHEMA IF NOT EXISTS payment;

-- Grant permissions
GRANT ALL PRIVILEGES ON SCHEMA security TO postgres;
GRANT ALL PRIVILEGES ON SCHEMA customer TO postgres;
GRANT ALL PRIVILEGES ON SCHEMA inventory TO postgres;
GRANT ALL PRIVILEGES ON SCHEMA payment TO postgres;

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Optional: Create read-only user for reporting
CREATE USER rardi_reader WITH PASSWORD 'reader_password';
GRANT CONNECT ON DATABASE rardi_db TO rardi_reader;
GRANT USAGE ON SCHEMA security, customer, inventory, payment TO rardi_reader;
GRANT SELECT ON ALL TABLES IN SCHEMA security, customer, inventory, payment TO rardi_reader;

-- Set default privileges for future tables
ALTER DEFAULT PRIVILEGES IN SCHEMA security GRANT SELECT ON TABLES TO rardi_reader;
ALTER DEFAULT PRIVILEGES IN SCHEMA customer GRANT SELECT ON TABLES TO rardi_reader;
ALTER DEFAULT PRIVILEGES IN SCHEMA inventory GRANT SELECT ON TABLES TO rardi_reader;
ALTER DEFAULT PRIVILEGES IN SCHEMA payment GRANT SELECT ON TABLES TO rardi_reader;