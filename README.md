# Testable Demo Project

Showcase several ways to test modern applications by example of a .NET service and a React SPA.

## Usage

### Create database

```sql
podman run --name test-db -p 5432:5432 -e POSTGRES_PASSWORD=postgres -e POSTGRES_USER=postgres -d postgres
```