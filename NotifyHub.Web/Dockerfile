# Stage 1 — Build Angular app
# Updated to Node 20 — required by Angular 17+
FROM node:20-alpine AS build
WORKDIR /app

# Copy package files first for layer caching
COPY package*.json ./
RUN npm install

# Copy source and build
COPY . .
RUN npm run build -- --configuration production

# Stage 2 — Serve with Nginx
FROM nginx:alpine AS runtime

# Copy built Angular files
COPY --from=build /app/dist/NotifyHub.Web/browser /usr/share/nginx/html

# Copy custom Nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80