#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/SimpleCQRS.Views/SimpleCQRS.Redis.Views.csproj", "src/SimpleCQRS.Views/"]
RUN dotnet restore "src/SimpleCQRS.Views/SimpleCQRS.Redis.Views.csproj"
COPY . .
WORKDIR "/src/src/SimpleCQRS.Views"
RUN dotnet build "SimpleCQRS.Redis.Views.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SimpleCQRS.Redis.Views.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleCQRS.Redis.Views.dll"]