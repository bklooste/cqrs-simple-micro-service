docker run -d --name eventstore-logic-itest -p 2114:2114 -p 1114:1114 -e EVENTSTORE_RUN_PROJECTIONS=ALL -e EVENTSTORE_START_STANDARD_PROJECTIONS=TRUE eventstore/eventstore
docker start eventstore-logic-itest
REM below requires latest eg release build
docker run -d --name simplecqrsapi-itest -e "DOTNET_USE_POLLING_FILE_WATCHER=1" -e "ConnectionStrings:EventStoreConnection=ConnectTo=tcp://admin:changeit@host.docker.internal:1114;HeartBeatTimeout=500" -p 53104:80 simplecqrsapi:latest
docker start simplecqrsapi-itest
timeout 15
dotnet test
docker stop eventstore-logic-itest
docker stop simplecqrsapi-itest

 