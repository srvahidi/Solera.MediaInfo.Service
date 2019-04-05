FROM microsoft/dotnet:2.1-sdk
 
RUN apt-get update && \
	apt-get install zip unzip redis-server -y && \
    curl -L "https://cli.run.pivotal.io/stable?release=linux64-binary" | tar -zx -C /usr/local/bin && \
	curl https://raw.githubusercontent.com/pivotalservices/artifactory-resource/master/tools/jq -o /usr/local/bin/jq && \
	chmod a+x /usr/local/bin/jq
