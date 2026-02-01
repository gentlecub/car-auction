# ============================================
# CarAuction Frontend - Railway Dockerfile
# Optimized for Railway.app deployment
# ============================================

# ============================================
# Stage 1: Dependencies
# ============================================
FROM node:20-alpine AS deps
WORKDIR /app

# Install dependencies only (for better caching)
COPY package.json package-lock.json* ./
RUN npm ci --legacy-peer-deps

# ============================================
# Stage 2: Builder
# ============================================
FROM node:20-alpine AS builder
WORKDIR /app

# Copy dependencies from deps stage
COPY --from=deps /app/node_modules ./node_modules
COPY . .

# Build arguments for environment variables
# These will be passed from Railway's environment variables
ARG VITE_API_URL
ARG VITE_WS_URL
ARG VITE_ENV=production

# Set environment variables for build
ENV VITE_API_URL=$VITE_API_URL
ENV VITE_WS_URL=$VITE_WS_URL
ENV VITE_ENV=$VITE_ENV

# Build the application
RUN npm run build

# ============================================
# Stage 3: Production Runner (Nginx)
# ============================================
FROM nginx:1.25-alpine AS runner

# Install curl for health checks and envsubst for config templating
RUN apk add --no-cache curl gettext

# Remove default nginx config
RUN rm /etc/nginx/conf.d/default.conf

# Copy nginx template configuration (uses $PORT)
COPY nginx/nginx.railway.conf /etc/nginx/templates/default.conf.template

# Copy built application from builder stage
COPY --from=builder /app/dist /usr/share/nginx/html

# Create necessary directories with proper permissions
RUN chown -R nginx:nginx /usr/share/nginx/html && \
    chown -R nginx:nginx /var/cache/nginx && \
    chown -R nginx:nginx /var/log/nginx && \
    mkdir -p /var/run/nginx && \
    chown -R nginx:nginx /var/run/nginx && \
    chown -R nginx:nginx /etc/nginx/conf.d

# Railway provides PORT environment variable
ENV PORT=80

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:${PORT}/health || exit 1

# Start script: substitute PORT in nginx config, then start nginx
CMD envsubst '${PORT}' < /etc/nginx/templates/default.conf.template > /etc/nginx/conf.d/default.conf && nginx -g 'daemon off;'
