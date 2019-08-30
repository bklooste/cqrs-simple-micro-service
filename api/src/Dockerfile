FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["SimpleCQRS.API/SimpleCQRS.API.csproj", "SimpleCQRS.API/"]
RUN dotnet restore "SimpleCQRS.API/SimpleCQRS.API.csproj"
COPY . .
WORKDIR "/src/SimpleCQRS.API"
RUN dotnet build "SimpleCQRS.API.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "SimpleCQRS.API.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SimpleCQRS.API.dll"]