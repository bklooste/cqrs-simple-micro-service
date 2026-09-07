FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/SimpleCQRS.Views/SimpleCQRS.Views.csproj", "src/SimpleCQRS.Views/"]
RUN dotnet restore "src/SimpleCQRS.Views/SimpleCQRS.Views.csproj"
COPY . .
WORKDIR "/src/src/SimpleCQRS.Views"
RUN dotnet build "SimpleCQRS.Views.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "SimpleCQRS.Views.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SimpleCQRS.Views.dll"]