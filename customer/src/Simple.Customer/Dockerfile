FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["src/SimpleCQRS.API/SimpleCQRS.API.csproj", "src/SimpleCQRS.API/"]
RUN dotnet restore "src/SimpleCQRS.API/SimpleCQRS.API.csproj"
COPY . .
WORKDIR "/src/src/SimpleCQRS.API"
RUN dotnet build "SimpleCQRS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SimpleCQRS.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleCQRS.API.dll"]