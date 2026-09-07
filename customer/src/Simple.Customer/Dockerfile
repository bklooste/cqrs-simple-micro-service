FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Simple.Customer/Simple.Customer.csproj", "src/Simple.Customer/"]
RUN dotnet restore "src/Simple.Customer/Simple.Customer.csproj"
COPY . .
WORKDIR "/src/src/Simple.Customer"
RUN dotnet build "Simple.Customer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Simple.Customer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Simple.Customer.dll"]