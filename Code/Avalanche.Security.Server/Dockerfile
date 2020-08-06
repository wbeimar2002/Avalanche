#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 81
EXPOSE 444

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Avalanche.Security.Server/Avalanche.Security.Server.csproj", "Avalanche.Security.Server/"]
RUN dotnet restore "Avalanche.Security.Server/Avalanche.Security.Server.csproj"

COPY . .
WORKDIR "/src/Avalanche.Security.Server"
RUN dotnet build "Avalanche.Security.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Avalanche.Security.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Avalanche.Security.Server.dll"]