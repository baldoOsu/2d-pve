FROM node:20.1.0-alpine3.17 as ts-compiler
WORKDIR /usr/app
COPY ./backend/package*.json ./
COPY ./backend/tsconfig*.json ./
RUN npm install
RUN npm install typescript
COPY ./backend ./
RUN npm run build

FROM node:20.1.0-alpine3.17 as ts-remover
WORKDIR /usr/app
COPY --from=ts-compiler /usr/app/package*.json ./
COPY --from=ts-compiler /usr/app/dist ./dist/
RUN npm install --only=production

FROM gcr.io/distroless/nodejs20-debian11
WORKDIR /usr/app
COPY --from=ts-remover /usr/app ./
USER 1000
EXPOSE 3000 3001 3006
CMD ["dist/index.js"]