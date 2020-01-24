#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Avalanche.Api/Avalanche.Api.csproj", "Avalanche.Api/"]
COPY ["Avalanche.Shared.Infrastructure/Avalanche.Shared.Infrastructure.csproj", "Avalanche.Shared.Infrastructure/"]
COPY ["Avalanche.Shared.Domain/Avalanche.Shared.Domain.csproj", "Avalanche.Shared.Domain/"]
RUN dotnet restore "Avalanche.Api/Avalanche.Api.csproj"
COPY . .
WORKDIR "/src/Avalanche.Api"
RUN dotnet build "Avalanche.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Avalanche.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Avalanche.Api.dll"]
