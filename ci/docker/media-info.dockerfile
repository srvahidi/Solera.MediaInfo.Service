FROM feeds.axadmin.net/docker/dotnet/core/aspnet:2.1

ENV ASPNETCORE_URLS=http://+:8080
WORKDIR /service-deployables
COPY service-deployables/ ./

# using a non-root user is a best practice for security related execution.
RUN useradd --uid $(shuf -i 2000-65000 -n 1) mediainfoApp
USER mediainfoApp

ENTRYPOINT ["dotnet", "Solera.MediaInfo.Service.dll"]
