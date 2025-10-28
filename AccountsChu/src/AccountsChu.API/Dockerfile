# Build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copiar arquivos de projeto e restaurar depend�ncias
COPY AccountsChu.sln ./
COPY src/AccountsChu.API/AccountsChu.API.csproj src/AccountsChu.API/
COPY src/AccountsChu.Domain/AccountsChu.Domain.csproj src/AccountsChu.Domain/
COPY src/AccountsChu.Infrastructure/AccountsChu.Infrastructure.csproj src/AccountsChu.Infrastructure/
COPY src/AccountsChu.Tests/AccountsChu.Tests.csproj src/AccountsChu.Tests/
RUN dotnet restore AccountsChu.sln


COPY . .

# Publica a aplica��o
WORKDIR "/src/src/AccountsChu.API"
RUN dotnet publish "AccountsChu.API.csproj" -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=build /app/publish .


ENTRYPOINT ["dotnet", "AccountsChu.API.dll"]