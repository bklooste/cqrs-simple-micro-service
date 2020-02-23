docker-compose up -d
timeout 5
dotnet test
docker-compose down
