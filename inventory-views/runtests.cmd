SET EVENTSTORE_HOST_TCP_PORT=1115
SET EVENTSTORE_CONNECTION=ConnectTo=tcp://admin:changeit@host.docker.internal:%EVENTSTORE_HOST_TCP_PORT%;HeartBeatTimeout=500
SET InventoryViewsServicePort=53105

docker run -d --name eventstore-views-itest -p 2115:2113 -p %EVENTSTORE_HOST_TCP_PORT%:1113 -e EVENTSTORE_RUN_PROJECTIONS=ALL -e EVENTSTORE_START_STANDARD_PROJECTIONS=TRUE eventstore/eventstore
docker start eventstore-views-itest
docker run -d --name simplecqrs-views-itest -e "DOTNET_USE_POLLING_FILE_WATCHER=1" -e ConnectionStrings:EventStoreConnection=%EVENTSTORE_CONNECTION%  -p %InventoryViewsServicePort%:80 simplecqrsviews:latest
docker start simplecqrs-views-itest
timeout 15
dotnet test

REM if you dont want to run in Visual studio you can stop the container
REM docker stop eventstore-views-itest
REM docker stop simplecqrs-views-itest

 