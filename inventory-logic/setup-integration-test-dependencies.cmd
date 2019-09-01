docker run --name eventstore-node -dt -p 2113:2113 -p 1113:1113 eventstore/eventstore
docker start eventstore-node
REM below requires latest eg release build
docker run -dt -e "DOTNET_USE_POLLING_FILE_WATCHER=1" -p 53104:80 --entrypoint tail simplecqrsapi:latest
REM docker run -dt -e "DOTNET_USE_POLLING_FILE_WATCHER=1" -p 53105:80 --entrypoint tail simplecqrs-view:dev -f /dev/null
timeout 15