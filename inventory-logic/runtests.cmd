SET EVENTSTORE_HOST_TCP_PORT=1114
SET EVENTSTORE_CONNECTION=ConnectTo=tcp://admin:changeit@host.docker.internal:%EVENTSTORE_HOST_TCP_PORT%;HeartBeatTimeout=500
SET InventoryLogicServicePort=53104

docker run -d --name eventstore-logic-itest -p 2114:2113 -p %EVENTSTORE_HOST_TCP_PORT%:1113 -e EVENTSTORE_RUN_PROJECTIONS=ALL -e EVENTSTORE_START_STANDARD_PROJECTIONS=TRUE eventstore/eventstore
docker start eventstore-logic-itest
docker run -d --name simplecqrs-logic-itest -e "DOTNET_USE_POLLING_FILE_WATCHER=1" -e ConnectionStrings:EventStoreConnection=%EVENTSTORE_CONNECTION%  -p %InventoryLogicServicePort%:80 simplecqrsapi:latest
docker start simplecqrs-logic-itest
timeout 15
dotnet test

REM if you dont want to run in Visual studio you can stop the container
REM docker stop eventstore-logic-itest
REM docker stop simplecqrs-logic-itest

 