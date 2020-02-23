dotnet build -c Release
docker build -t simplecqrsapi-redis -f src\SimpleCQRS.API\Dockerfile  .
