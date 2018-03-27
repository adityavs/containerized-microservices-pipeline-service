FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app

COPY ./LoginService ./
RUN dotnet publish -c Debug -o compiledOutput

ENV ASPNETCORE_URLS=http://+:4201
EXPOSE 4201

WORKDIR compiledOutput
ENTRYPOINT [ "dotnet", "LoginService.dll" ]