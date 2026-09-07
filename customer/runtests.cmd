SET CustomerServicePort=54104
SET POSTGRES_CONNECTION=Host=host.docker.internal;Port=5432;Database=postgres;Username=postgres;Password=mysecretpassword

docker run --name cust-postgres -p 5432:5432 -e POSTGRES_PASSWORD=mysecretpassword -d postgres:16-alpine
docker start cust-postgres
docker run -d --name customer-itest -e "DOTNET_USE_POLLING_FILE_WATCHER=1" -e ConnectionStrings:PostgresConnection=%POSTGRES_CONNECTION%  -p %CustomerServicePort%:80 customer:latest
docker start customer-itest
timeout 15
dotnet test

REM if you dont want to run in Visual studio you can stop the containers
REM docker stop cust-postgres
REM docker stop customer-itest
