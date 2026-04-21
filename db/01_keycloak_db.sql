-- Creates the dedicated Keycloak database if it does not already exist.
-- This script is run by the postgres entrypoint (docker-entrypoint-initdb.d)
-- as the superuser, so it can CREATE DATABASE freely.
SELECT 'CREATE DATABASE keycloak_db OWNER postgres'
WHERE NOT EXISTS (
    SELECT FROM pg_database WHERE datname = 'keycloak_db'
)\gexec
