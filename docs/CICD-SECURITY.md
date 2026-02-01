# CICD-SECURITY.md — SAST, Secrets Scan y Seguridad

## Capas de Seguridad en CI/CD

```
┌─────────────────────────────────────────────────────────────────┐
│                      SECURITY LAYERS                             │
├─────────────────────────────────────────────────────────────────┤
│  1. Pre-commit     │ Secrets detection local (git-secrets)      │
│  2. CI - SAST      │ Static analysis código fuente              │
│  3. CI - SCA       │ Vulnerabilidades dependencias              │
│  4. CI - Secrets   │ Scan de credenciales hardcodeadas          │
│  5. CI - Container │ Scan imagen Docker                         │
│  6. CD - DAST      │ (Opcional) Pruebas dinámicas               │
└─────────────────────────────────────────────────────────────────┘
```

---

## Workflow Security Completo

```yaml
# /.github/workflows/security-scan.yml
name: Security Scan

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]
  schedule:
    # Ejecutar semanalmente
    - cron: '0 0 * * 0'

jobs:
  # ============================================
  # SECRET SCANNING
  # ============================================
  secrets-scan:
    name: Secrets Detection
    runs-on: ubuntu-latest

    steps:
      - name: Checkout code
        uses: actions/checkout@v4
        with:
          fetch-depth: 0  # Full history para detectar en commits

      - name: TruffleHog Secret Scan
        uses: trufflesecurity/trufflehog@main
        with:
          path: ./
          base: ${{ github.event.repository.default_branch }}
          extra_args: --only-verified

      - name: Gitleaks Scan
        uses: gitleaks/gitleaks-action@v2
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}

  # ============================================
  # DEPENDENCY VULNERABILITY SCAN
  # ============================================
  dependency-scan:
    name: Dependency Vulnerabilities
    runs-on: ubuntu-latest

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      # Backend (.NET)
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: .NET Security Scan
        run: |
          dotnet tool install --global dotnet-outdated-tool
          dotnet restore backend/CarAuction.sln
          dotnet list backend/CarAuction.sln package --vulnerable --include-transitive
        continue-on-error: true

      # Frontend (npm)
      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'

      - name: npm audit
        run: |
          cd frontend
          npm ci
          npm audit --audit-level=high
        continue-on-error: true

      - name: Snyk Scan
        uses: snyk/actions/node@master
        continue-on-error: true
        env:
          SNYK_TOKEN: ${{ secrets.SNYK_TOKEN }}
        with:
          args: --all-projects --severity-threshold=high

  # ============================================
  # STATIC CODE ANALYSIS (SAST)
  # ============================================
  sast-scan:
    name: SAST Analysis
    runs-on: ubuntu-latest

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Initialize CodeQL
        uses: github/codeql-action/init@v3
        with:
          languages: csharp, javascript

      - name: Autobuild
        uses: github/codeql-action/autobuild@v3

      - name: Perform CodeQL Analysis
        uses: github/codeql-action/analyze@v3
        with:
          category: "/language:csharp"

      - name: Semgrep SAST
        uses: returntocorp/semgrep-action@v1
        with:
          config: >-
            p/default
            p/security-audit
            p/secrets
            p/owasp-top-ten

  # ============================================
  # CONTAINER SCAN
  # ============================================
  container-scan:
    name: Container Vulnerability Scan
    runs-on: ubuntu-latest
    needs: [secrets-scan]
    if: github.event_name == 'push'

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Build Backend Image
        run: docker build -t carauction-api:scan ./backend

      - name: Build Frontend Image
        run: docker build -t carauction-frontend:scan ./frontend

      - name: Trivy Backend Scan
        uses: aquasecurity/trivy-action@master
        with:
          image-ref: 'carauction-api:scan'
          format: 'sarif'
          output: 'trivy-backend.sarif'
          severity: 'HIGH,CRITICAL'

      - name: Trivy Frontend Scan
        uses: aquasecurity/trivy-action@master
        with:
          image-ref: 'carauction-frontend:scan'
          format: 'sarif'
          output: 'trivy-frontend.sarif'
          severity: 'HIGH,CRITICAL'

      - name: Upload Trivy Results
        uses: github/codeql-action/upload-sarif@v3
        if: always()
        with:
          sarif_file: 'trivy-backend.sarif'
```

---

## Pre-commit Hooks (Local)

```yaml
# /.pre-commit-config.yaml
repos:
  - repo: https://github.com/pre-commit/pre-commit-hooks
    rev: v4.5.0
    hooks:
      - id: check-yaml
      - id: end-of-file-fixer
      - id: trailing-whitespace
      - id: check-added-large-files
        args: ['--maxkb=500']

  - repo: https://github.com/gitleaks/gitleaks
    rev: v8.18.0
    hooks:
      - id: gitleaks

  - repo: https://github.com/awslabs/git-secrets
    rev: master
    hooks:
      - id: git-secrets
```

```bash
# Instalar pre-commit
pip install pre-commit
pre-commit install
```

---

## Dependabot Configuration

```yaml
# /.github/dependabot.yml
version: 2
updates:
  # Backend NuGet
  - package-ecosystem: "nuget"
    directory: "/backend"
    schedule:
      interval: "weekly"
    open-pull-requests-limit: 5
    labels:
      - "dependencies"
      - "backend"

  # Frontend npm
  - package-ecosystem: "npm"
    directory: "/frontend"
    schedule:
      interval: "weekly"
    open-pull-requests-limit: 5
    labels:
      - "dependencies"
      - "frontend"

  # GitHub Actions
  - package-ecosystem: "github-actions"
    directory: "/"
    schedule:
      interval: "weekly"
    labels:
      - "ci/cd"
```

---

## Checklist de Seguridad CI/CD

| # | Control | Herramienta | Estado |
|---|---------|-------------|--------|
| 1 | Secrets en código | Gitleaks/TruffleHog | ⬜ |
| 2 | Vulnerabilidades deps | npm audit / dotnet | ⬜ |
| 3 | SAST código | CodeQL / Semgrep | ⬜ |
| 4 | Container scan | Trivy | ⬜ |
| 5 | Pre-commit hooks | git-secrets | ⬜ |
| 6 | Dependabot activo | GitHub | ⬜ |
| 7 | Branch protection | GitHub Settings | ⬜ |

---

**🛑 CONTINÚA leyendo para Módulo 3.5: Deploy Strategies**
