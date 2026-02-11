# Stage 1: Build Angular UI
FROM node:22-alpine AS ui-build
WORKDIR /app
COPY src/NeonBoard.UI/package*.json ./
RUN npm ci
COPY src/NeonBoard.UI/ ./
RUN npm run build

# Stage 2: Build .NET API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api-build
WORKDIR /app

# Copy project files and restore dependencies (layer caching)
COPY Directory.Build.Props Directory.Packages.Props NeonBoard.slnx ./
COPY src/NeonBoard.Api/NeonBoard.Api.csproj src/NeonBoard.Api/
COPY src/NeonBoard.Application/NeonBoard.Application.csproj src/NeonBoard.Application/
COPY src/NeonBoard.Domain/NeonBoard.Domain.csproj src/NeonBoard.Domain/
COPY src/NeonBoard.Infrastructure/NeonBoard.Infrastructure.csproj src/NeonBoard.Infrastructure/
COPY src/NeonBoard.ServiceDefaults/NeonBoard.ServiceDefaults.csproj src/NeonBoard.ServiceDefaults/
RUN dotnet restore src/NeonBoard.Api/NeonBoard.Api.csproj

# Copy source and publish
COPY src/ src/
RUN dotnet publish src/NeonBoard.Api/NeonBoard.Api.csproj -c Release -o /app/publish --no-restore

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=api-build /app/publish .
COPY --from=ui-build /app/dist/neonboard/browser wwwroot/

EXPOSE 8080
ENTRYPOINT ["dotnet", "NeonBoard.Api.dll"]
