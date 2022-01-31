FROM feeds.axadmin.net/docker/dotnet/core/aspnet:2.1

# using a non-root user is a best practice for security related execution.
ARG APP_USER=mediainfoApp
RUN useradd --uid $(shuf -i 2000-65000 -n 1) $APP_USER
USER $APP_USER

ENV ASPNETCORE_URLS=http://+:8080
WORKDIR /service-deployables
COPY --chown=$APP_USER service-deployables/ ./

ENTRYPOINT ["dotnet", "Solera.MediaInfo.Service.dll"]
