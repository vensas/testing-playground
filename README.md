# Testable Demo Project

Showcase several ways to test modern applications by example of a .NET service and a React SPA.

## Disclaimer

- Demo project for test types, ignore configuration/security settings/code style

## Prerequisites

- [.NET 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- [React 19](https://reactjs.org/)
- [Node.js](https://nodejs.org/)
- [Podman](https://podman.io/getting-started/installation)

## Test Frameworks used
- [Jest](https://jestjs.io)
- [React Testing Library](https://testing-library.com/docs/react-testing-library/intro/)
- [XUnit](https://xunit.net/)
- [TestContainers](https://testcontainers.com/)
- [Playwright](https://playwright.dev)
- [Axe](https://www.deque.com/axe/)
- [k6](https://grafana.com/docs/k6/latest/set-up/install-k6/)

## Usage

### Create and run database

```sql
podman run --name test-db -p 5432:5432 -e POSTGRES_PASSWORD=postgres -e POSTGRES_USER=postgres -d postgres
```

### Run the backend service

```sh
dotnet run --project src/backend/Testable.Api/Testable.Api.csproj
```

### Configure the frontend application

Create a `.env` file in the `src/frontend` directory with the following content:

```properties
BACKEND_URL=http://localhost:5266
```

### Run the frontend application

```sh
cd src/frontend
npm install
npm start
```

## Run backend unit and integration tests
```sh
cd src/backend
dotnet test
```

## Run frontend unit/component tests
```sh
cd src/frontend
npm install
npm run test
```

## Run frontend playwright tests
```sh
cd src/frontend
npm install
npm run playwright
```

## Run k6 performance tests

```sh
cd src/k6
k6 run test.js
```