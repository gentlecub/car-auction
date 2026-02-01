# CICD-FRONTEND.md — CI Pipeline React + Vite

## Workflow Completo

```yaml
# /.github/workflows/ci-frontend.yml
name: CI Frontend

on:
  push:
    branches: [main, develop]
    paths:
      - 'frontend/**'
      - '.github/workflows/ci-frontend.yml'
  pull_request:
    branches: [main, develop]
    paths:
      - 'frontend/**'

defaults:
  run:
    working-directory: frontend

env:
  NODE_VERSION: '20'

jobs:
  # ============================================
  # LINT & TYPE CHECK
  # ============================================
  lint:
    name: Lint & Format
    runs-on: ubuntu-latest

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}
          cache: 'npm'
          cache-dependency-path: frontend/package-lock.json

      - name: Install dependencies
        run: npm ci

      - name: Run ESLint
        run: npm run lint
        continue-on-error: true

      - name: Check Prettier formatting
        run: npm run format:check
        continue-on-error: true

      - name: TypeScript type check
        run: npm run type-check
        continue-on-error: true

  # ============================================
  # UNIT TESTS
  # ============================================
  test:
    name: Unit Tests
    runs-on: ubuntu-latest

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}
          cache: 'npm'
          cache-dependency-path: frontend/package-lock.json

      - name: Install dependencies
        run: npm ci

      - name: Run unit tests
        run: npm run test -- --coverage --watchAll=false

      - name: Upload coverage
        uses: codecov/codecov-action@v3
        if: github.event_name != 'pull_request'
        with:
          directory: frontend/coverage
          flags: frontend
          fail_ci_if_error: false

  # ============================================
  # BUILD
  # ============================================
  build:
    name: Build
    runs-on: ubuntu-latest
    needs: [lint, test]

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}
          cache: 'npm'
          cache-dependency-path: frontend/package-lock.json

      - name: Install dependencies
        run: npm ci

      - name: Build application
        run: npm run build
        env:
          VITE_API_URL: ${{ vars.VITE_API_URL || 'http://localhost:5000/api' }}
          VITE_WS_URL: ${{ vars.VITE_WS_URL || 'ws://localhost:5000/hubs/auction' }}

      - name: Upload build artifact
        uses: actions/upload-artifact@v4
        with:
          name: frontend-build
          path: frontend/dist
          retention-days: 7

  # ============================================
  # DOCKER BUILD
  # ============================================
  docker:
    name: Docker Build
    runs-on: ubuntu-latest
    needs: build
    if: github.event_name == 'push'

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@v3

      - name: Build Docker image
        uses: docker/build-push-action@v5
        with:
          context: ./frontend
          push: false
          tags: carauction-frontend:${{ github.sha }}
          build-args: |
            VITE_API_URL=${{ vars.VITE_API_URL }}
            VITE_WS_URL=${{ vars.VITE_WS_URL }}
          cache-from: type=gha
          cache-to: type=gha,mode=max

  # ============================================
  # SECURITY AUDIT
  # ============================================
  security:
    name: Security Audit
    runs-on: ubuntu-latest
    if: github.event_name == 'push'

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}
          cache: 'npm'
          cache-dependency-path: frontend/package-lock.json

      - name: Install dependencies
        run: npm ci

      - name: Run npm audit
        run: npm audit --audit-level=high
        continue-on-error: true

      - name: Check for known vulnerabilities
        uses: snyk/actions/node@master
        continue-on-error: true
        env:
          SNYK_TOKEN: ${{ secrets.SNYK_TOKEN }}
        with:
          args: --severity-threshold=high
```

---

## Scripts package.json Requeridos

```json
{
  "scripts": {
    "dev": "vite",
    "build": "vite build",
    "lint": "eslint src --ext .ts,.tsx --report-unused-disable-directives --max-warnings 0",
    "lint:fix": "eslint src --ext .ts,.tsx --fix",
    "format": "prettier --write \"src/**/*.{ts,tsx,css,json}\"",
    "format:check": "prettier --check \"src/**/*.{ts,tsx,css,json}\"",
    "type-check": "tsc --noEmit",
    "test": "vitest",
    "test:coverage": "vitest --coverage"
  }
}
```

---

## Dependencias Dev Requeridas

```bash
cd frontend
npm install -D \
  eslint \
  eslint-plugin-react \
  eslint-plugin-react-hooks \
  @typescript-eslint/eslint-plugin \
  @typescript-eslint/parser \
  prettier \
  vitest \
  @vitest/coverage-v8 \
  jsdom \
  @testing-library/react \
  @testing-library/jest-dom \
  typescript
```

---

## Configuración ESLint

```javascript
// frontend/.eslintrc.cjs
module.exports = {
  root: true,
  env: { browser: true, es2020: true },
  extends: [
    'eslint:recommended',
    'plugin:@typescript-eslint/recommended',
    'plugin:react-hooks/recommended',
  ],
  ignorePatterns: ['dist', '.eslintrc.cjs'],
  parser: '@typescript-eslint/parser',
  plugins: ['react-refresh'],
  rules: {
    'react-refresh/only-export-components': [
      'warn',
      { allowConstantExport: true },
    ],
  },
};
```

---

## Tiempos Esperados

| Job | Duración |
|-----|----------|
| Lint | ~30s |
| Test | ~1-2min |
| Build | ~1min |
| Docker | ~2min |
| Security | ~1min |
| **Total** | **~5-6min** |

---

**🛑 CONTINÚA leyendo para Módulo 3.3: Tests Automáticos**
