#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["src/SimpleCQRS.API/SimpleCQRS.Redis.API.csproj", "src/SimpleCQRS.API/"]
RUN dotnet restore "src/SimpleCQRS.API/SimpleCQRS.Redis.API.csproj"
COPY . .
WORKDIR "/src/src/SimpleCQRS.API"
RUN dotnet build "SimpleCQRS.Redis.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SimpleCQRS.Redis.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleCQRS.Redis.API.dll"]