FROM feeds.axadmin.net/docker/dotnet/core/aspnet:2.1

ENV ASPNETCORE_URLS=http://+:8080
WORKDIR /deployables
COPY deployables/ ./

ENTRYPOINT ["dotnet", "Solera.MediaInfo.Service.dll"]
