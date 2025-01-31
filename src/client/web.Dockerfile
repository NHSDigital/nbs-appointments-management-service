FROM node:20-alpine

WORKDIR /app

COPY package.json ./
COPY package-lock.json ./
COPY .husky ./.husky
RUN npm ci;

COPY src ./src
COPY public ./public
COPY next.config.mjs .
COPY tsconfig.json .

ENV NEXT_TELEMETRY_DISABLED 1

CMD npm run start;