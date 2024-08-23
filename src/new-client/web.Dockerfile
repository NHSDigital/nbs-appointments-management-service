FROM node:20-alpine

WORKDIR /app

COPY src/new-client/package.json ./
COPY src/new-client/package-lock.json ./
RUN npm ci;

COPY src/new-client/src ./src
COPY src/new-client/public ./public
COPY src/new-client/next.config.mjs .
COPY src/new-client/tsconfig.json .

ENV NEXT_TELEMETRY_DISABLED 1

CMD npm run dev;