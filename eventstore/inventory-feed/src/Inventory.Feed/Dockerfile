FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Inventory.Feed/Inventory.Feed.csproj", "src/Inventory.Feed/"]
RUN dotnet restore "src/Inventory.Feed/Inventory.Feed.csproj"
COPY . .
WORKDIR "/src/src/Inventory.Feed"
RUN dotnet build "Inventory.Feed.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Inventory.Feed.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Inventory.Feed.dll"]