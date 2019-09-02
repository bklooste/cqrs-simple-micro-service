dotnet build -c Release
docker build -t simplecqrsapi -f src\SimpleCQRS.API\Dockerfile  .
