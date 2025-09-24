# Estágio 1: Build da Aplicação
FROM node:20-alpine AS builder

# Define o diretório de trabalho
WORKDIR /app

# Copia os arquivos de dependência e instala
COPY package*.json ./
RUN npm install

# Copia o restante do código da aplicação
COPY . .

# Expõe a variável de ambiente para o processo de build
ARG NEXT_PUBLIC_API_URL
ENV NEXT_PUBLIC_API_URL=${NEXT_PUBLIC_API_URL}

# Constrói a aplicação Next.js
RUN npm run build

# Estágio 2: Produção - Imagem final otimizada
FROM node:20-alpine

WORKDIR /app

# Copia os arquivos de dependência de produção
COPY --from=builder /app/package*.json ./
RUN npm install --omit=dev

# Copia a build da aplicação
COPY --from=builder /app/.next ./.next
COPY --from=builder /app/public ./public
COPY --from=builder /app/next.config.mjs ./

# Expõe a porta que o Next.js usa
EXPOSE 3000

# Comando para iniciar a aplicação em produção
CMD ["npm", "start"]