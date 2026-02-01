# ============================================
# CarAuction Frontend - Railway Dockerfile
# ============================================

FROM node:25-alpine AS builder
WORKDIR /app

# Copy package files
COPY package.json package-lock.json* ./

# Install ALL dependencies (including devDependencies for build)
RUN npm ci --legacy-peer-deps

# Copy source code
COPY . .

# Build arguments for environment variables
ARG VITE_API_URL
ARG VITE_WS_URL
ARG VITE_ENV=production

ENV VITE_API_URL=$VITE_API_URL
ENV VITE_WS_URL=$VITE_WS_URL
ENV VITE_ENV=$VITE_ENV

# Build the application
RUN npm run build

# Stage 2: Serve with Node
FROM node:25-alpine AS runner
WORKDIR /app

# Install serve globally
RUN npm install -g serve

# Copy built files
COPY --from=builder /app/dist ./dist

# Railway provides PORT, default to 3000
ENV PORT=3000

EXPOSE ${PORT}

# Serve the app
CMD ["sh", "-c", "serve -s dist -l ${PORT}"]
