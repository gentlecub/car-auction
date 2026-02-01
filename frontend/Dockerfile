# ============================================
# CarAuction Frontend - Railway Dockerfile
# ============================================

# Stage 1: Dependencies
FROM node:20-alpine AS deps
WORKDIR /app
COPY package.json package-lock.json* ./
RUN npm ci --legacy-peer-deps

# Stage 2: Builder
FROM node:20-alpine AS builder
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .

ARG VITE_API_URL
ARG VITE_WS_URL
ARG VITE_ENV=production

ENV VITE_API_URL=$VITE_API_URL
ENV VITE_WS_URL=$VITE_WS_URL
ENV VITE_ENV=$VITE_ENV

RUN npm run build

# Stage 3: Serve with Node (simpler than Nginx for Railway)
FROM node:20-alpine AS runner
WORKDIR /app

# Install serve globally
RUN npm install -g serve

# Copy built files
COPY --from=builder /app/dist ./dist

# Railway provides PORT, default to 3000
ENV PORT=3000

EXPOSE ${PORT}

# Health check
HEALTHCHECK --interval=10s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:${PORT}/ || exit 1

# Serve the app - serve automatically uses PORT env var
CMD ["sh", "-c", "serve -s dist -l ${PORT}"]
