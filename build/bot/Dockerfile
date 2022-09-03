FROM mcr.microsoft.com/dotnet/runtime:5.0-alpine AS runtime
WORKDIR /app
COPY out ./
ENTRYPOINT ["dotnet", "Pyrewatcher.dll"]