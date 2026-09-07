SET EVENTSTORE_HOST_GRPC_PORT=2115
SET EVENTSTORE_CONNECTION=esdb://admin:changeit@host.docker.internal:%EVENTSTORE_HOST_GRPC_PORT%?tls=false
SET InventoryViewsServicePort=53105

docker run -d --name eventstore-views-itest -p %EVENTSTORE_HOST_GRPC_PORT%:2113 -e EVENTSTORE_INSECURE=true -e EVENTSTORE_CLUSTER_SIZE=1 -e EVENTSTORE_RUN_PROJECTIONS=All -e EVENTSTORE_START_STANDARD_PROJECTIONS=true -e EVENTSTORE_MEM_DB=true eventstore/eventstore:23.10.1-bookworm-slim
docker start eventstore-views-itest
docker run -d --name simplecqrs-views-itest -e "DOTNET_USE_POLLING_FILE_WATCHER=1" -e ConnectionStrings:EventStoreConnection=%EVENTSTORE_CONNECTION%  -p %InventoryViewsServicePort%:80 simplecqrsviews:latest
docker start simplecqrs-views-itest
timeout 15
dotnet test

REM if you dont want to run in Visual studio you can stop the container
REM docker stop eventstore-views-itest
REM docker stop simplecqrs-views-itest
