FROM mcr.microsoft.com/dotnet/core/runtime:2.1-alpine

RUN apk -f -q update && apk -f -q add bash curl git jq grep zip unzip vim && \
    curl -L "https://cli.run.pivotal.io/stable?release=linux64-binary" | tar -zx -C /usr/local/bin && \
	curl https://raw.githubusercontent.com/pivotalservices/artifactory-resource/master/tools/jq -o /usr/local/bin/jq && \
	chmod a+x /usr/local/bin/jq
