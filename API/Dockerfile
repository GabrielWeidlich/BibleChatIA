# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["API/BibleChat.Api.csproj", "API/"]
RUN dotnet restore "API/BibleChat.Api.csproj"
COPY . .
WORKDIR "/src/API"
RUN dotnet build "BibleChat.Api.csproj" -c Release -o /app/build

# Estágio de Publicação
FROM build AS publish
RUN dotnet publish "BibleChat.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio Final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

COPY API/Prompts ./Prompts

# A instrução EXPOSE documenta qual porta o contêiner escuta.
EXPOSE 8080

ENTRYPOINT ["dotnet", "BibleChat.Api.dll"]
