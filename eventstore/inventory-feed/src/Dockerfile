FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Inventory.Feed/Inventory.Feed.csproj", "Inventory.Feed/"]
RUN dotnet restore "Inventory.Feed/Inventory.Feed.csproj"
COPY . .
WORKDIR "/src/Inventory.Feed"
RUN dotnet build "Inventory.Feed.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Inventory.Feed.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Inventory.Feed.dll"]