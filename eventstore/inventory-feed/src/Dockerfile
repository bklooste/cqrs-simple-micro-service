FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["SimpleCQRS.Views/SimpleCQRS.Views.csproj", "SimpleCQRS.Views/"]
RUN dotnet restore "SimpleCQRS.Views/SimpleCQRS.Views.csproj"
COPY . .
WORKDIR "/src/SimpleCQRS.Views"
RUN dotnet build "SimpleCQRS.Views.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "SimpleCQRS.Views.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SimpleCQRS.Views.dll"]