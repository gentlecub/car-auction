# CarAuction

A full-stack real-time car auction platform built with React and ASP.NET Core 8.

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)
![React](https://img.shields.io/badge/React-18-blue.svg)
![Docker](https://img.shields.io/badge/Docker-Ready-blue.svg)

## Overview

CarAuction is a modern web application for online vehicle auctions featuring real-time bidding, user authentication, and an admin dashboard. Built with enterprise-grade architecture and production-ready CI/CD pipelines.

### Key Features

- **Real-time Bidding** — Live bid updates via SignalR WebSockets
- **User Authentication** — JWT-based auth with refresh tokens
- **Auction Management** — Create, manage, and monitor auctions
- **Admin Dashboard** — Analytics, user management, and reports
- **Responsive Design** — Mobile-first UI with Tailwind CSS
- **Containerized** — Docker support for all environments

---

## Tech Stack

### Frontend
| Technology | Purpose |
|------------|---------|
| React 18 | UI Framework |
| Vite | Build Tool |
| TypeScript | Type Safety |
| Tailwind CSS 4 | Styling |
| Radix UI | Accessible Components |
| SignalR Client | Real-time Communication |
| Axios | HTTP Client |

### Backend
| Technology | Purpose |
|------------|---------|
| ASP.NET Core 8 | Web API |
| Entity Framework Core | ORM |
| MySQL 8 | Database |
| Redis | Caching & SignalR Backplane |
| SignalR | WebSocket Hub |
| JWT | Authentication |

### DevOps
| Technology | Purpose |
|------------|---------|
| Docker | Containerization |
| GitHub Actions | CI/CD |
| Nginx | Reverse Proxy |

---

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         Client Browser                          │
└───────────────────────────┬─────────────────────────────────────┘
                            │
              ┌─────────────┴─────────────┐
              │                           │
              ▼                           ▼
    ┌─────────────────┐         ┌─────────────────┐
    │    Frontend     │         │   SignalR Hub   │
    │  React + Nginx  │         │   (WebSocket)   │
    │     :3000       │         │     :5000       │
    └────────┬────────┘         └────────┬────────┘
             │                           │
             └───────────┬───────────────┘
                         │
                         ▼
              ┌─────────────────────┐
              │    ASP.NET Core     │
              │     Web API         │
              │       :5000         │
              └──────────┬──────────┘
                         │
           ┌─────────────┴─────────────┐
           │                           │
           ▼                           ▼
    ┌─────────────┐           ┌─────────────┐
    │    MySQL    │           │    Redis    │
    │    :3306    │           │    :6379    │
    └─────────────┘           └─────────────┘
```

### Project Structure

```
car-auction/
├── frontend/                 # React application
│   ├── src/
│   │   ├── app/             # Components, views, data
│   │   ├── services/        # API clients, hooks, context
│   │   └── styles/          # CSS files
│   ├── nginx/               # Nginx configuration
│   ├── Dockerfile           # Production build
│   └── Dockerfile.dev       # Development build
│
├── backend/                  # ASP.NET Core API
│   ├── src/
│   │   ├── CarAuction.API/          # Controllers, middleware
│   │   ├── CarAuction.Application/  # Business logic, DTOs
│   │   ├── CarAuction.Domain/       # Entities, interfaces
│   │   └── CarAuction.Infrastructure/ # EF Core, repositories
│   ├── tests/               # Unit & integration tests
│   ├── Dockerfile           # Production build
│   └── Dockerfile.dev       # Development build
│
├── docs/                     # Documentation
├── .github/                  # CI/CD workflows
├── docker-compose.yml        # Production stack
└── docker-compose.dev.yml    # Development stack
```

---

## Getting Started

### Prerequisites

- [Docker](https://www.docker.com/) 20.10+
- [Node.js](https://nodejs.org/) 20+ (for local development)
- [.NET SDK](https://dotnet.microsoft.com/) 8.0 (for local development)

### Quick Start with Docker

```bash
# Clone the repository
git clone https://github.com/your-username/car-auction.git
cd car-auction

# Start all services
docker-compose -f docker-compose.dev.yml up -d

# View logs
docker-compose -f docker-compose.dev.yml logs -f
```

**Access the application:**

| Service | URL |
|---------|-----|
| Frontend | http://localhost:5173 |
| API | http://localhost:5000 |
| Swagger | http://localhost:5000/swagger |

### Local Development (Without Docker)

#### Frontend

```bash
cd frontend
npm install
npm run dev
```

#### Backend

```bash
cd backend
dotnet restore
dotnet run --project src/CarAuction.API
```

---

## Configuration

### Environment Variables

#### Frontend (`.env.development`)

```env
VITE_API_URL=http://localhost:5000/api
VITE_WS_URL=ws://localhost:5000/hubs/auction
VITE_ENV=development
```

#### Backend

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Development` |
| `ConnectionStrings__DefaultConnection` | MySQL connection string | — |
| `JwtSettings__SecretKey` | JWT signing key (min 32 chars) | — |
| `JwtSettings__Issuer` | JWT issuer | `CarAuction` |
| `JwtSettings__Audience` | JWT audience | `CarAuctionClient` |
| `Redis__ConnectionString` | Redis connection | `localhost:6379` |
| `Cors__Origins` | Allowed CORS origins | `http://localhost:5173` |

---

## API Documentation

API documentation is available via Swagger UI at `/swagger` when running in development mode.

### Main Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/login` | User login |
| POST | `/api/auth/register` | User registration |
| GET | `/api/auctions` | List auctions |
| GET | `/api/auctions/{id}` | Get auction details |
| POST | `/api/bids` | Place a bid |
| GET | `/api/cars` | List cars |

### WebSocket Events (SignalR)

| Event | Direction | Description |
|-------|-----------|-------------|
| `BidPlaced` | Server → Client | New bid notification |
| `AuctionUpdated` | Server → Client | Auction state changed |
| `AuctionEnded` | Server → Client | Auction completed |

---

## Docker

### Development

```bash
# Start all services with hot reload
docker-compose -f docker-compose.dev.yml up -d

# Rebuild specific service
docker-compose -f docker-compose.dev.yml up -d --build frontend

# Stop all services
docker-compose -f docker-compose.dev.yml down
```

### Production

```bash
# Build and start production stack
docker-compose up -d --build

# With custom environment
docker-compose --env-file .env.production up -d
```

### Build Images Individually

```bash
# Frontend
docker build -t carauction-frontend:latest \
  --build-arg VITE_API_URL=https://api.example.com \
  ./frontend

# Backend
docker build -t carauction-api:latest ./backend
```

---

## CI/CD

This project includes GitHub Actions workflows for continuous integration and deployment.

### Workflows

| Workflow | Trigger | Description |
|----------|---------|-------------|
| `ci-frontend.yml` | Push/PR to frontend | Lint, test, build, Docker |
| `ci-backend.yml` | Push/PR to backend | Build, test, Docker |
| `security-scan.yml` | Push/PR + Weekly | SAST, secrets, vulnerabilities |
| `cd-deploy.yml` | Push to main | Deploy to staging/production |

### Required Secrets

```
STAGING_HOST        # Staging server hostname
STAGING_USER        # SSH username
STAGING_SSH_KEY     # SSH private key
PROD_HOST           # Production server hostname
PROD_USER           # SSH username
PROD_SSH_KEY        # SSH private key
```

---

## Testing

### Backend

```bash
cd backend

# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Frontend

```bash
cd frontend

# Run tests
npm run test

# Run with coverage
npm run test -- --coverage
```

---

## Documentation

Additional documentation is available in the `/docs` directory:

| Document | Description |
|----------|-------------|
| [INTEGRATION.md](./INTEGRATION.md) | Frontend-Backend integration guide |
| [DEV-SETUP.md](./docs/DEV-SETUP.md) | Development environment setup |
| [DOCKER-FRONTEND.md](./docs/DOCKER-FRONTEND.md) | Frontend Docker configuration |
| [DOCKER-BACKEND.md](./docs/DOCKER-BACKEND.md) | Backend Docker configuration |
| [CICD-OVERVIEW.md](./docs/CICD-OVERVIEW.md) | CI/CD pipeline architecture |
| [QA-CHECKLIST.md](./docs/QA-CHECKLIST.md) | Quality assurance checklist |
| [QA-PRODUCTION-READY.md](./docs/QA-PRODUCTION-READY.md) | Production readiness criteria |

---

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

Please ensure your PR:
- Passes all CI checks
- Includes tests for new functionality
- Updates documentation as needed

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## Acknowledgments

- [Radix UI](https://www.radix-ui.com/) for accessible components
- [Tailwind CSS](https://tailwindcss.com/) for styling
- [SignalR](https://dotnet.microsoft.com/apps/aspnet/signalr) for real-time features

---

Built with :heart: by the CarAuction Team
