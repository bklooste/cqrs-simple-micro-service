docker run -d --name eventstore-view -p 2113:2113 -p 1113:1113 -e EVENTSTORE_RUN_PROJECTIONS=ALL -e EVENTSTORE_START_STANDARD_PROJECTIONS=TRUE eventstore/eventstore
docker start eventstore-view
REM below requires latest eg release build
docker run -d -e "DOTNET_USE_POLLING_FILE_WATCHER=1" -e "ConnectionStrings:EventStoreConnection=ConnectTo=tcp://admin:changeit@host.docker.internal:1113;HeartBeatTimeout=500" -p 53104:80 simplecqrsapi:latest
REM docker run -dt -e "DOTNET_USE_POLLING_FILE_WATCHER=1" -p 53104:80 --entrypoint tail simplecqrs-view:dev -f /dev/null
timeout 15