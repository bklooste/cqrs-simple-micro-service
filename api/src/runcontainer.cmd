docker run -dt -e "DOTNET_USE_POLLING_FILE_WATCHER=1" - -p 53104:80 --entrypoint tail simplecqrsapi:dev -f /dev/null
