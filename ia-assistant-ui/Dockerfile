# Etapa de build
FROM node:20-alpine AS build
WORKDIR /app
COPY . .
RUN npm install
RUN npm run build

# Etapa de producción
FROM nginx:alpine
COPY --from=build /app/dist/ia-assistant-ui /usr/share/nginx/html
COPY ./nginx.conf /etc/nginx/conf.d/default.conf
