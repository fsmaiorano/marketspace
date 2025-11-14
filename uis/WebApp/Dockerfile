FROM node:22-bullseye-slim AS builder
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends \
    build-essential python3 ca-certificates \
  && rm -rf /var/lib/apt/lists/*

COPY uis/WebApp/package*.json ./

RUN npm ci --legacy-peer-deps

COPY uis/WebApp/ .
RUN npm run build:ssr

FROM node:22-bullseye-slim AS runner
WORKDIR /app

ENV PORT=4000
ENV NODE_ENV=production

COPY --from=builder /app/dist/WebApp /app/dist/WebApp
COPY --from=builder /app/package*.json ./

RUN npm ci --omit=dev --legacy-peer-deps

USER node

EXPOSE 4000

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD node -e "const http=require('http'); const req=http.request({method:'GET',port:process.env.PORT||4000,path:'/'},res=>{ if(res.statusCode<200||res.statusCode>=400) process.exit(1); process.exit(0); }); req.on('error',()=>process.exit(1)); req.end();"

CMD ["node", "dist/WebApp/server/server.mjs"]
