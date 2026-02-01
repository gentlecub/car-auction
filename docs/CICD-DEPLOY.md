# CICD-DEPLOY.md — Estrategias de Despliegue

## Opciones de Deployment

```
┌─────────────────────────────────────────────────────────────────┐
│                    DEPLOYMENT OPTIONS                            │
├─────────────────┬───────────────┬───────────────┬───────────────┤
│     VPS/VM      │  Kubernetes   │   Serverless  │    PaaS       │
│  (SSH Deploy)   │   (K8s/EKS)   │  (Cloud Run)  │  (Railway)    │
├─────────────────┼───────────────┼───────────────┼───────────────┤
│ ✅ Simple       │ ✅ Escalable  │ ✅ Auto-scale │ ✅ Zero-config│
│ ✅ Económico    │ ✅ HA nativo  │ ✅ Pay-per-use│ ✅ Rápido     │
│ ❌ Manual scale │ ❌ Complejo   │ ❌ Cold starts│ ❌ Vendor lock│
└─────────────────┴───────────────┴───────────────┴───────────────┘
```

---

## 1. Deploy a VPS (SSH) — Actual

El workflow `cd.yml` existente usa SSH deploy:

```yaml
deploy-staging:
  steps:
    - name: Deploy via SSH
      uses: appleboy/ssh-action@v1.0.0
      with:
        host: ${{ secrets.STAGING_HOST }}
        username: ${{ secrets.STAGING_USER }}
        key: ${{ secrets.STAGING_SSH_KEY }}
        script: |
          cd /opt/carauction
          docker-compose pull
          docker-compose up -d --remove-orphans
          docker image prune -f
```

### Script de Deploy Mejorado

```bash
#!/bin/bash
# /opt/carauction/deploy.sh

set -e

COMPOSE_FILE="docker-compose.yml"
BACKUP_DIR="/opt/backups"

echo "=== Starting deployment $(date) ==="

# 1. Backup database
echo "Creating database backup..."
docker-compose exec -T mysql mysqldump -u root -p"${MYSQL_ROOT_PASSWORD}" carauction > \
  "${BACKUP_DIR}/db_$(date +%Y%m%d_%H%M%S).sql"

# 2. Pull new images
echo "Pulling new images..."
docker-compose pull

# 3. Deploy with zero-downtime
echo "Deploying services..."
docker-compose up -d --remove-orphans --scale api=2

# 4. Wait for health
echo "Waiting for health check..."
sleep 30
curl -f http://localhost:5000/health || exit 1

# 5. Scale down old container
docker-compose up -d --scale api=1

# 6. Cleanup
docker image prune -f

echo "=== Deployment completed $(date) ==="
```

---

## 2. Deploy a GitHub Container Registry

```yaml
# CD workflow - Push to GHCR
build-and-push:
  steps:
    - name: Login to GHCR
      uses: docker/login-action@v3
      with:
        registry: ghcr.io
        username: ${{ github.actor }}
        password: ${{ secrets.GITHUB_TOKEN }}

    - name: Build and Push API
      uses: docker/build-push-action@v5
      with:
        context: ./backend
        push: true
        tags: |
          ghcr.io/${{ github.repository }}/api:${{ github.sha }}
          ghcr.io/${{ github.repository }}/api:latest

    - name: Build and Push Frontend
      uses: docker/build-push-action@v5
      with:
        context: ./frontend
        push: true
        tags: |
          ghcr.io/${{ github.repository }}/frontend:${{ github.sha }}
          ghcr.io/${{ github.repository }}/frontend:latest
        build-args: |
          VITE_API_URL=${{ vars.VITE_API_URL }}
```

---

## 3. Deploy a AWS (ECS/ECR)

```yaml
# /.github/workflows/cd-aws.yml
deploy-aws:
  steps:
    - name: Configure AWS credentials
      uses: aws-actions/configure-aws-credentials@v4
      with:
        aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
        aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
        aws-region: us-east-1

    - name: Login to Amazon ECR
      id: login-ecr
      uses: aws-actions/amazon-ecr-login@v2

    - name: Build and push to ECR
      env:
        ECR_REGISTRY: ${{ steps.login-ecr.outputs.registry }}
        IMAGE_TAG: ${{ github.sha }}
      run: |
        docker build -t $ECR_REGISTRY/carauction-api:$IMAGE_TAG ./backend
        docker push $ECR_REGISTRY/carauction-api:$IMAGE_TAG

    - name: Deploy to ECS
      uses: aws-actions/amazon-ecs-deploy-task-definition@v1
      with:
        task-definition: .aws/task-definition.json
        service: carauction-service
        cluster: carauction-cluster
        wait-for-service-stability: true
```

---

## 4. Deploy a Google Cloud Run

```yaml
# /.github/workflows/cd-gcp.yml
deploy-cloudrun:
  steps:
    - name: Authenticate to Google Cloud
      uses: google-github-actions/auth@v2
      with:
        credentials_json: ${{ secrets.GCP_SA_KEY }}

    - name: Setup Cloud SDK
      uses: google-github-actions/setup-gcloud@v2

    - name: Build and Push to GCR
      run: |
        gcloud builds submit --tag gcr.io/${{ vars.GCP_PROJECT }}/carauction-api ./backend

    - name: Deploy to Cloud Run
      run: |
        gcloud run deploy carauction-api \
          --image gcr.io/${{ vars.GCP_PROJECT }}/carauction-api \
          --region us-central1 \
          --platform managed \
          --allow-unauthenticated \
          --set-env-vars "ASPNETCORE_ENVIRONMENT=Production"
```

---

## 5. Rollback Strategy

```yaml
rollback:
  name: Rollback Deployment
  runs-on: ubuntu-latest
  if: failure()

  steps:
    - name: Rollback via SSH
      uses: appleboy/ssh-action@v1.0.0
      with:
        host: ${{ secrets.PROD_HOST }}
        username: ${{ secrets.PROD_USER }}
        key: ${{ secrets.PROD_SSH_KEY }}
        script: |
          cd /opt/carauction

          # Obtener imagen anterior
          PREV_IMAGE=$(docker images --format "{{.Repository}}:{{.Tag}}" | head -2 | tail -1)

          # Rollback
          docker-compose down
          docker tag $PREV_IMAGE carauction-api:latest
          docker-compose up -d

          echo "Rollback completed to $PREV_IMAGE"
```

---

## Matriz de Comparación

| Aspecto | VPS (SSH) | AWS ECS | GCP Run | Railway |
|---------|-----------|---------|---------|---------|
| Costo mensual | $20-50 | $50-200 | Pay/use | $5-50 |
| Complejidad | Baja | Alta | Media | Muy baja |
| Auto-scaling | Manual | ✅ | ✅ | ✅ |
| SSL/TLS | Manual | ✅ | ✅ | ✅ |
| Database | Self-hosted | RDS | Cloud SQL | Incluido |
| Recomendado | MVP/Startups | Enterprise | Microservices | Side projects |

---

**FASE 3 COMPLETADA**

---

**🛑 DETENTE — Escribe `CONTINUAR` para FASE 4: Validación y Calidad**
