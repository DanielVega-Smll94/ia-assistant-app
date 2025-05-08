# Multi-stage build para .NET 9 backend y Angular frontend

# --- BACKEND ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-backend
WORKDIR /src
COPY ./OpenAiApiDemo/OpenAiApiDemo.csproj ./OpenAiApiDemo/
RUN dotnet restore ./OpenAiApiDemo/OpenAiApiDemo.csproj
COPY ./OpenAiApiDemo ./OpenAiApiDemo
WORKDIR /src/OpenAiApiDemo
RUN dotnet publish -c Release -o /app/backend

# --- FRONTEND ---
FROM node:20-alpine AS build-frontend
WORKDIR /app
COPY ./ia-assistant-ui ./
RUN npm install
RUN npm run build --prod

# --- FINAL IMAGE ---
FROM nginx:alpine
WORKDIR /app

# ✅ Este path depende de angular.json "outputPath": "dist"
COPY --from=build-frontend /app/dist /usr/share/nginx/html

# ✅ Backend copia completa
COPY --from=build-backend /app/backend /app/backend

# ✅ nginx.conf con Angular routes fallback
COPY ./ia-assistant-ui/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
EXPOSE 5000

# ✅ Levantar ambos servicios: backend + nginx
CMD ["sh", "-c", "/app/backend/OpenAiApiDemo & nginx -g 'daemon off;'"]

