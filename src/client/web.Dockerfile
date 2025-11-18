FROM node:22-alpine

WORKDIR /app

COPY package.json ./
COPY package-lock.json ./
RUN npm ci;

COPY src ./src
COPY public ./public
COPY next.config.mjs .
COPY tsconfig.json .

CMD npm run start;