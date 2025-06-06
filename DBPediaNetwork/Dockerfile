# Estágio de build
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

# Copiar arquivos de projeto e restaurar dependências
COPY ["DBPediaNetwork.csproj", "./"]
RUN dotnet restore

# Copiar o resto do código
COPY . .
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

# Estágio final/runtime
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Configurar variáveis de ambiente
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Expor a porta usada pela aplicação
EXPOSE 80

# Comando para executar a aplicação
ENTRYPOINT ["dotnet", "DBPediaNetwork.dll"]
