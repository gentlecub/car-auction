# QA-INTEGRATION-TESTS.md — Pruebas de Integración Frontend ↔ Backend

## Flujos Críticos a Validar

```
┌─────────────────────────────────────────────────────────────────┐
│                    CRITICAL USER FLOWS                          │
├─────────────────────────────────────────────────────────────────┤
│  1. Authentication    │ Register → Login → Token → Protected   │
│  2. Auction Browsing  │ List → Filter → Detail → Images        │
│  3. Bidding           │ Auth → Place Bid → Real-time Update    │
│  4. Admin Panel       │ Auth Admin → CRUD → Reports            │
└─────────────────────────────────────────────────────────────────┘
```

---

## 1. Test de Autenticación

### Backend Integration Test

```csharp
// tests/CarAuction.IntegrationTests/AuthEndpointsTests.cs
[Collection("Integration")]
public class AuthEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var loginRequest = new { email = "test@test.com", password = "Test123!" };
        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        // Act
        var response = await _client.GetAsync("/api/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithValidToken_Returns200()
    {
        // Arrange - Login first
        var token = await GetValidTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

### Frontend Integration Test

```typescript
// frontend/src/__tests__/integration/auth.integration.test.ts
import { describe, it, expect, beforeAll, afterAll } from 'vitest';
import axios from 'axios';

const API_URL = process.env.VITE_API_URL || 'http://localhost:5000/api';

describe('Authentication Integration', () => {
  let accessToken: string;

  it('should login with valid credentials', async () => {
    const response = await axios.post(`${API_URL}/auth/login`, {
      email: 'test@test.com',
      password: 'Test123!',
    });

    expect(response.status).toBe(200);
    expect(response.data.accessToken).toBeDefined();
    accessToken = response.data.accessToken;
  });

  it('should access protected endpoint with token', async () => {
    const response = await axios.get(`${API_URL}/users/me`, {
      headers: { Authorization: `Bearer ${accessToken}` },
    });

    expect(response.status).toBe(200);
    expect(response.data.email).toBe('test@test.com');
  });

  it('should reject invalid token', async () => {
    try {
      await axios.get(`${API_URL}/users/me`, {
        headers: { Authorization: 'Bearer invalid-token' },
      });
    } catch (error: any) {
      expect(error.response.status).toBe(401);
    }
  });
});
```

---

## 2. Test de Flujo de Subastas

```typescript
// frontend/src/__tests__/integration/auction.integration.test.ts
describe('Auction Flow Integration', () => {
  let accessToken: string;

  beforeAll(async () => {
    // Login
    const login = await axios.post(`${API_URL}/auth/login`, {
      email: 'bidder@test.com',
      password: 'Test123!',
    });
    accessToken = login.data.accessToken;
  });

  it('should list active auctions', async () => {
    const response = await axios.get(`${API_URL}/auctions?status=active`);

    expect(response.status).toBe(200);
    expect(Array.isArray(response.data.items)).toBe(true);
  });

  it('should get auction details with car info', async () => {
    const response = await axios.get(`${API_URL}/auctions/1`);

    expect(response.status).toBe(200);
    expect(response.data.car).toBeDefined();
    expect(response.data.car.brand).toBeDefined();
    expect(response.data.currentBid).toBeGreaterThanOrEqual(0);
  });

  it('should place a bid on active auction', async () => {
    const currentAuction = await axios.get(`${API_URL}/auctions/1`);
    const newBidAmount = currentAuction.data.currentBid + 500;

    const response = await axios.post(
      `${API_URL}/bids`,
      { auctionId: 1, amount: newBidAmount },
      { headers: { Authorization: `Bearer ${accessToken}` } }
    );

    expect(response.status).toBe(201);
    expect(response.data.amount).toBe(newBidAmount);
  });

  it('should reject bid lower than current', async () => {
    try {
      await axios.post(
        `${API_URL}/bids`,
        { auctionId: 1, amount: 100 }, // Very low bid
        { headers: { Authorization: `Bearer ${accessToken}` } }
      );
    } catch (error: any) {
      expect(error.response.status).toBe(400);
    }
  });
});
```

---

## 3. Test E2E con Playwright

```typescript
// e2e/tests/auction-flow.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Auction User Journey', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('complete bidding flow', async ({ page }) => {
    // 1. Login
    await page.click('text=Iniciar Sesión');
    await page.fill('[name="email"]', 'bidder@test.com');
    await page.fill('[name="password"]', 'Test123!');
    await page.click('button[type="submit"]');

    // 2. Wait for redirect to home
    await expect(page).toHaveURL('/');
    await expect(page.locator('text=Mi Cuenta')).toBeVisible();

    // 3. Browse auctions
    await page.click('.car-card >> nth=0');
    await expect(page.locator('.auction-detail')).toBeVisible();

    // 4. Place bid
    const currentBid = await page.locator('.current-bid').textContent();
    const newBid = parseInt(currentBid!.replace(/\D/g, '')) + 500;

    await page.fill('[name="bidAmount"]', newBid.toString());
    await page.click('text=Realizar Puja');

    // 5. Verify bid placed
    await expect(page.locator('.toast-success')).toBeVisible();
    await expect(page.locator('.current-bid')).toContainText(newBid.toString());
  });

  test('unauthorized user cannot bid', async ({ page }) => {
    // Go directly to auction without login
    await page.goto('/auction/1');

    // Try to bid
    await page.fill('[name="bidAmount"]', '10000');
    await page.click('text=Realizar Puja');

    // Should redirect to login
    await expect(page).toHaveURL(/.*login/);
  });
});
```

---

## 4. Script de Prueba Manual

```bash
#!/bin/bash
# /scripts/integration-test.sh

API_URL="http://localhost:5000/api"
FRONTEND_URL="http://localhost:3000"

echo "=== Integration Test Suite ==="

# Test 1: Backend Health
echo -n "1. Backend Health: "
HEALTH=$(curl -s ${API_URL}/health)
[ "$HEALTH" == "Healthy" ] && echo "✅ PASS" || echo "❌ FAIL"

# Test 2: Login
echo -n "2. Login API: "
TOKEN=$(curl -s -X POST ${API_URL}/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","password":"Test123!"}' \
  | jq -r '.accessToken')
[ -n "$TOKEN" ] && echo "✅ PASS" || echo "❌ FAIL"

# Test 3: Protected Endpoint
echo -n "3. Protected Endpoint: "
USER=$(curl -s ${API_URL}/users/me \
  -H "Authorization: Bearer $TOKEN" \
  | jq -r '.email')
[ "$USER" == "test@test.com" ] && echo "✅ PASS" || echo "❌ FAIL"

# Test 4: List Auctions
echo -n "4. List Auctions: "
AUCTIONS=$(curl -s ${API_URL}/auctions | jq '.items | length')
[ "$AUCTIONS" -gt 0 ] && echo "✅ PASS ($AUCTIONS found)" || echo "❌ FAIL"

# Test 5: Frontend Loads
echo -n "5. Frontend Loads: "
FRONTEND=$(curl -s -o /dev/null -w "%{http_code}" $FRONTEND_URL)
[ "$FRONTEND" == "200" ] && echo "✅ PASS" || echo "❌ FAIL"

echo "=== Tests Complete ==="
```

---

**🛑 CONTINÚA leyendo para Módulo 4.3: Validación CORS/JWT/Red**
