docker run -d -e "DOTNET_USE_POLLING_FILE_WATCHER=1" -e "ConnectionStrings:EventStoreConnection=ConnectTo=tcp://admin:changeit@host.docker.internal:1113;HeartBeatTimeout=500" -p 53104:80 simplecqrsapi:latest

