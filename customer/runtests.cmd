SET EVENTSTORE_HOST_TCP_PORT=1114
SET EVENTSTORE_CONNECTION=ConnectTo=tcp://admin:changeit@host.docker.internal:%EVENTSTORE_HOST_TCP_PORT%;HeartBeatTimeout=500
SET InventoryLogicServicePort=53104


REM admin postgress on port 8045
REM docker run -p 8045:80   -e "PGADMIN_DEFAULT_EMAIL=user@domain.com"   -e "PGADMIN_DEFAULT_PASSWORD=mysecretpassword"   -d dpage/pgadmin4
REM docker run --name cust-postgres -e POSTGRES_PASSWORD=mysecretpassword -p 5432:5432 -it clkao/postgres-plv8
REM docker start  cust-postgres
REM  docker exec cust-postgres psql -U postgres -c 'CREATE EXTENSION IF NOT EXISTS plv8;'
REM docker exec cust-postgres psql -U postgres -c "SELECT * FROM pg_extension"


docker run --name cust-postgres -p 5432:5432 -e POSTGRES_PASSWORD=mysecretpassword -d clkao/postgres-plv8 psql -U postgres -c "SELECT plv8_version();" 
REM docker exec cust-postgres psql -U postgres -c "SELECT plv8_version();"
docker start cust-postgres
docker run -d --name customer-itest -e "DOTNET_USE_POLLING_FILE_WATCHER=1" -e ConnectionStrings:EventStoreConnection=%EVENTSTORE_CONNECTION%  -p %InventoryLogicServicePort%:80 customer:latest
docker start customer-itest
timeout 15
dotnet test

REM if you dont want to run in Visual studio you can stop the container
REM docker stop eventstore-logic-itest
REM docker stop simplecqrs-logic-itest

 