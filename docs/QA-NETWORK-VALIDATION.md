# QA-NETWORK-VALIDATION.md — Validación CORS, JWT y Red

## 1. Validación CORS

### Diagnóstico Rápido

```bash
# Test CORS preflight desde terminal
curl -X OPTIONS http://localhost:5000/api/auctions \
  -H "Origin: http://localhost:3000" \
  -H "Access-Control-Request-Method: GET" \
  -H "Access-Control-Request-Headers: Authorization" \
  -v 2>&1 | grep -E "Access-Control"
```

**Respuesta esperada:**
```
< Access-Control-Allow-Origin: http://localhost:3000
< Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS
< Access-Control-Allow-Headers: Authorization, Content-Type
< Access-Control-Allow-Credentials: true
```

### Checklist CORS

| # | Verificación | Comando | Estado |
|---|--------------|---------|--------|
| 1 | Origin permitido | `curl -H "Origin: http://localhost:3000"` | ⬜ |
| 2 | Origin rechazado | `curl -H "Origin: http://malicious.com"` → No header | ⬜ |
| 3 | Preflight OPTIONS | `curl -X OPTIONS` → 200/204 | ⬜ |
| 4 | Credentials allowed | `Access-Control-Allow-Credentials: true` | ⬜ |
| 5 | Headers permitidos | Authorization en Allow-Headers | ⬜ |

### Test CORS desde Browser Console

```javascript
// Ejecutar en DevTools del frontend
fetch('http://localhost:5000/api/auctions', {
  method: 'GET',
  headers: { 'Authorization': 'Bearer test' },
  credentials: 'include'
})
.then(r => console.log('CORS OK:', r.status))
.catch(e => console.error('CORS ERROR:', e));
```

---

## 2. Validación JWT

### Estructura del Token

```bash
# Decodificar JWT (sin verificar firma)
TOKEN="eyJhbGciOiJIUzI1NiIs..."

# Header
echo $TOKEN | cut -d'.' -f1 | base64 -d 2>/dev/null | jq

# Payload
echo $TOKEN | cut -d'.' -f2 | base64 -d 2>/dev/null | jq
```

**Payload esperado:**
```json
{
  "sub": "user-id-123",
  "email": "user@test.com",
  "role": "User",
  "iat": 1706745600,
  "exp": 1706749200,
  "iss": "CarAuction",
  "aud": "CarAuctionClient"
}
```

### Checklist JWT

| # | Verificación | Método | Estado |
|---|--------------|--------|--------|
| 1 | Token válido acepta | GET `/api/users/me` → 200 | ⬜ |
| 2 | Token expirado rechaza | Token exp pasado → 401 | ⬜ |
| 3 | Token inválido rechaza | Token modificado → 401 | ⬜ |
| 4 | Sin token rechaza | Sin header Auth → 401 | ⬜ |
| 5 | Refresh token funciona | POST `/api/auth/refresh` → nuevo token | ⬜ |
| 6 | Claims correctos | `role`, `email`, `sub` presentes | ⬜ |
| 7 | Issuer validado | `iss` = "CarAuction" | ⬜ |
| 8 | Audience validado | `aud` = "CarAuctionClient" | ⬜ |

### Script de Validación JWT

```bash
#!/bin/bash
# /scripts/validate-jwt.sh

API_URL="http://localhost:5000/api"

echo "=== JWT Validation Suite ==="

# 1. Obtener token válido
echo -n "1. Get valid token: "
RESPONSE=$(curl -s -X POST ${API_URL}/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","password":"Test123!"}')
TOKEN=$(echo $RESPONSE | jq -r '.accessToken')
REFRESH=$(echo $RESPONSE | jq -r '.refreshToken')
[ -n "$TOKEN" ] && echo "✅" || echo "❌"

# 2. Token válido acepta
echo -n "2. Valid token accepted: "
STATUS=$(curl -s -o /dev/null -w "%{http_code}" ${API_URL}/users/me \
  -H "Authorization: Bearer $TOKEN")
[ "$STATUS" == "200" ] && echo "✅" || echo "❌ (got $STATUS)"

# 3. Token inválido rechaza
echo -n "3. Invalid token rejected: "
STATUS=$(curl -s -o /dev/null -w "%{http_code}" ${API_URL}/users/me \
  -H "Authorization: Bearer invalid.token.here")
[ "$STATUS" == "401" ] && echo "✅" || echo "❌ (got $STATUS)"

# 4. Sin token rechaza
echo -n "4. No token rejected: "
STATUS=$(curl -s -o /dev/null -w "%{http_code}" ${API_URL}/users/me)
[ "$STATUS" == "401" ] && echo "✅" || echo "❌ (got $STATUS)"

# 5. Refresh token funciona
echo -n "5. Refresh token works: "
NEW_TOKEN=$(curl -s -X POST ${API_URL}/auth/refresh \
  -H "Content-Type: application/json" \
  -d "{\"refreshToken\":\"$REFRESH\"}" | jq -r '.accessToken')
[ -n "$NEW_TOKEN" ] && echo "✅" || echo "❌"

# 6. Verificar claims
echo -n "6. Token claims valid: "
PAYLOAD=$(echo $TOKEN | cut -d'.' -f2 | base64 -d 2>/dev/null)
EMAIL=$(echo $PAYLOAD | jq -r '.email')
ROLE=$(echo $PAYLOAD | jq -r '.role')
[ "$EMAIL" == "test@test.com" ] && echo "✅ (email=$EMAIL, role=$ROLE)" || echo "❌"

echo "=== Validation Complete ==="
```

---

## 3. Validación de Red

### Puertos y Conectividad

```bash
#!/bin/bash
# /scripts/validate-network.sh

echo "=== Network Validation ==="

# Puertos esperados
declare -A PORTS=(
  ["Frontend"]=3000
  ["API"]=5000
  ["MySQL"]=3306
  ["Redis"]=6379
)

for SERVICE in "${!PORTS[@]}"; do
  PORT=${PORTS[$SERVICE]}
  echo -n "$SERVICE (:$PORT): "
  nc -z localhost $PORT 2>/dev/null && echo "✅ OPEN" || echo "❌ CLOSED"
done

# Docker network
echo ""
echo "Docker Network:"
docker network inspect carauction-network --format '{{range .Containers}}{{.Name}}: {{.IPv4Address}}{{"\n"}}{{end}}'

# DNS interno
echo ""
echo "Internal DNS Resolution:"
docker-compose exec -T api ping -c 1 mysql 2>/dev/null && echo "API → MySQL: ✅" || echo "API → MySQL: ❌"
docker-compose exec -T api ping -c 1 redis 2>/dev/null && echo "API → Redis: ✅" || echo "API → Redis: ❌"
```

### Checklist de Red

| # | Verificación | Comando | Estado |
|---|--------------|---------|--------|
| 1 | Frontend accesible | `curl http://localhost:3000` | ⬜ |
| 2 | API accesible | `curl http://localhost:5000/health` | ⬜ |
| 3 | MySQL conecta | `docker-compose exec mysql mysqladmin ping` | ⬜ |
| 4 | Redis conecta | `docker-compose exec redis redis-cli ping` | ⬜ |
| 5 | API → MySQL | Conexión desde contenedor API | ⬜ |
| 6 | API → Redis | Conexión desde contenedor API | ⬜ |
| 7 | Frontend → API | CORS permite comunicación | ⬜ |
| 8 | SignalR WebSocket | `ws://localhost:5000/hubs/auction` | ⬜ |

### Test SignalR/WebSocket

```javascript
// Ejecutar en Browser DevTools
const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5000/hubs/auction", {
    accessTokenFactory: () => localStorage.getItem('accessToken')
  })
  .build();

connection.start()
  .then(() => console.log("SignalR Connected ✅"))
  .catch(err => console.error("SignalR Error ❌:", err));

// Test event
connection.on("BidPlaced", (data) => {
  console.log("Bid received:", data);
});
```

---

## 4. Troubleshooting Común

| Problema | Causa | Solución |
|----------|-------|----------|
| CORS blocked | Origin no permitido | Agregar origen a `CORS_ORIGINS` |
| 401 en todo | JWT secret diferente | Verificar `JWT_SECRET_KEY` en env |
| Connection refused | Servicio no corriendo | `docker-compose up -d` |
| Network timeout | Firewall/Docker network | Verificar `docker network ls` |
| WebSocket fails | CORS o proxy mal configurado | Verificar nginx y `AllowCredentials` |

---

**🛑 CONTINÚA leyendo para Módulo 4.4: Criterios Production-Ready**
