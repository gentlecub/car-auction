# CICD-TESTS.md — Estrategia de Testing Automatizado

## Pirámide de Tests

```
                    ┌───────────┐
                   /│   E2E     │\        ~5%   (Cypress/Playwright)
                  / │  Tests    │ \
                 /  └───────────┘  \
                /   ┌───────────┐   \
               /    │Integration│    \    ~15%  (API + DB)
              /     │  Tests    │     \
             /      └───────────┘      \
            /       ┌───────────┐       \
           /        │   Unit    │        \  ~80%  (Componentes/Servicios)
          /         │  Tests    │         \
         /          └───────────┘          \
        ──────────────────────────────────────
```

---

## Backend (.NET) — Tests Existentes

### Estructura

```
backend/tests/
├── CarAuction.UnitTests/
│   ├── Services/
│   │   ├── AuthServiceTests.cs
│   │   ├── AuctionServiceTests.cs
│   │   └── BidServiceTests.cs
│   └── Controllers/
│       └── AuthControllerTests.cs
└── CarAuction.IntegrationTests/
    ├── ApiTests/
    │   ├── AuthEndpointsTests.cs
    │   └── AuctionEndpointsTests.cs
    └── Fixtures/
        └── WebApplicationFactoryFixture.cs
```

### Ejecutar Tests Backend

```bash
# Unit tests
dotnet test tests/CarAuction.UnitTests --configuration Release

# Integration tests (requiere DB)
dotnet test tests/CarAuction.IntegrationTests --configuration Release

# Con coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
```

---

## Frontend (React) — Tests a Implementar

### Estructura Propuesta

```
frontend/src/
├── __tests__/
│   ├── setup.ts                 # Configuración global
│   └── utils/
│       └── test-utils.tsx       # Render helpers
├── app/
│   ├── components/
│   │   ├── CarCard/
│   │   │   ├── CarCard.tsx
│   │   │   └── CarCard.test.tsx
│   │   └── Header/
│   │       ├── Header.tsx
│   │       └── Header.test.tsx
│   └── views/
│       ├── Home/
│       │   ├── Home.tsx
│       │   └── Home.test.tsx
│       └── Login/
│           ├── Login.tsx
│           └── Login.test.tsx
└── services/
    └── api/
        ├── authService.ts
        └── authService.test.ts
```

### Configuración Vitest

```typescript
// frontend/vitest.config.ts
import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: ['./src/__tests__/setup.ts'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      exclude: ['node_modules/', 'src/__tests__/'],
    },
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
});
```

### Setup Tests

```typescript
// frontend/src/__tests__/setup.ts
import '@testing-library/jest-dom';
import { cleanup } from '@testing-library/react';
import { afterEach, vi } from 'vitest';

// Cleanup after each test
afterEach(() => {
  cleanup();
});

// Mock localStorage
const localStorageMock = {
  getItem: vi.fn(),
  setItem: vi.fn(),
  clear: vi.fn(),
  removeItem: vi.fn(),
};
global.localStorage = localStorageMock as any;

// Mock fetch
global.fetch = vi.fn();
```

### Ejemplo Test Componente

```typescript
// frontend/src/app/components/CarCard/CarCard.test.tsx
import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import CarCard from './CarCard';

const mockCar = {
  id: 1,
  brand: 'Toyota',
  model: 'Corolla',
  year: 2023,
  currentBid: 15000,
  images: ['/car1.jpg'],
};

describe('CarCard', () => {
  it('renders car information correctly', () => {
    render(<CarCard car={mockCar} />);

    expect(screen.getByText('Toyota Corolla')).toBeInTheDocument();
    expect(screen.getByText('2023')).toBeInTheDocument();
    expect(screen.getByText('$15,000')).toBeInTheDocument();
  });

  it('displays current bid', () => {
    render(<CarCard car={mockCar} />);

    expect(screen.getByText(/15,000/)).toBeInTheDocument();
  });
});
```

### Ejemplo Test Service

```typescript
// frontend/src/services/api/authService.test.ts
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { login, logout } from './authService';
import axiosInstance from './axiosInstance';

vi.mock('./axiosInstance');

describe('authService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('login', () => {
    it('sends credentials and returns token', async () => {
      const mockResponse = { data: { accessToken: 'test-token' } };
      vi.mocked(axiosInstance.post).mockResolvedValue(mockResponse);

      const result = await login('user@test.com', 'password123');

      expect(axiosInstance.post).toHaveBeenCalledWith('/auth/login', {
        email: 'user@test.com',
        password: 'password123',
      });
      expect(result.accessToken).toBe('test-token');
    });
  });
});
```

---

## Coverage Mínimo Requerido

| Capa | Mínimo | Objetivo |
|------|--------|----------|
| Services/Utils | 80% | 90% |
| Components | 70% | 80% |
| Views/Pages | 50% | 70% |
| **Global** | **70%** | **80%** |

---

## Comandos CI

```bash
# Frontend
npm run test -- --coverage --watchAll=false

# Backend
dotnet test --collect:"XPlat Code Coverage"

# Report combinado (Codecov flags)
# frontend → flag: frontend
# backend → flag: backend
```

---

**🛑 CONTINÚA leyendo para Módulo 3.4: Security Scanning**
