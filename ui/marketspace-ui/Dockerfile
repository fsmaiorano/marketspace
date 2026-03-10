# Build stage
FROM node:22-alpine AS build

WORKDIR /app

COPY package*.json ./
RUN npm install --legacy-peer-deps

COPY . .

# BFF URL is injected at image build time (e.g. docker build --build-arg VITE_BFF_API_URL=http://my-host:5100)
ARG VITE_BFF_API_URL=http://localhost:5100
ARG VITE_ENV=docker
ENV VITE_BFF_API_URL=$VITE_BFF_API_URL
ENV VITE_ENV=$VITE_ENV

RUN npm run build

# Serve stage
FROM nginx:alpine AS final

COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 4000

CMD ["nginx", "-g", "daemon off;"]
