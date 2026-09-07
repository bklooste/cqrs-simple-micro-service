SET EVENTSTORE_HOST_GRPC_PORT=2114
SET EVENTSTORE_CONNECTION=esdb://admin:changeit@host.docker.internal:%EVENTSTORE_HOST_GRPC_PORT%?tls=false
SET InventoryLogicServicePort=53104

docker run -d --name eventstore-logic-itest -p %EVENTSTORE_HOST_GRPC_PORT%:2113 -e EVENTSTORE_INSECURE=true -e EVENTSTORE_CLUSTER_SIZE=1 -e EVENTSTORE_RUN_PROJECTIONS=All -e EVENTSTORE_START_STANDARD_PROJECTIONS=true -e EVENTSTORE_MEM_DB=true eventstore/eventstore:23.10.1-bookworm-slim
docker start eventstore-logic-itest
docker run -d --name simplecqrs-logic-itest -e "DOTNET_USE_POLLING_FILE_WATCHER=1" -e ConnectionStrings:EventStoreConnection=%EVENTSTORE_CONNECTION%  -p %InventoryLogicServicePort%:80 simplecqrsapi:latest
docker start simplecqrs-logic-itest
timeout 15
dotnet test

REM if you dont want to run in Visual studio you can stop the container
REM docker stop eventstore-logic-itest
REM docker stop simplecqrs-logic-itest
