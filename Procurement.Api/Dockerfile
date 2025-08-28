# Procurement.Api Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Procurement.Api/Procurement.Api.csproj", "Procurement.Api/"]
COPY ["Procurement.Application/Procurement.Application.csproj", "Procurement.Application/"]
COPY ["Procurement.Domain/Procurement.Domain.csproj", "Procurement.Domain/"]
COPY ["Procurement.Infrastructure/Procurement.Infrastructure.csproj", "Procurement.Infrastructure/"]
COPY ["BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj", "BuildingBlocks.Messaging/"]
COPY ["BuildingBlocks.Persistence/BuildingBlocks.Persistence.csproj", "BuildingBlocks.Persistence/"]
COPY ["BuildingBlocks.Web/BuildingBlocks.Web.csproj", "BuildingBlocks.Web/"]
RUN dotnet restore "Procurement.Api/Procurement.Api.csproj"
COPY . .
WORKDIR "/src/Procurement.Api"
RUN dotnet publish "Procurement.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Procurement.Api.dll"]
