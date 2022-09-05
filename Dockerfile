# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.sln .
COPY *.csproj .
RUN dotnet restore

# copy everything else and build app
COPY . .
WORKDIR /source
RUN dotnet publish -c release -o /app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim
COPY ./public ./../app/public
WORKDIR /app
COPY --from=build /app ./
#ENTRYPOINT ["dotnet", "StripeDemoOKR.dll"]
CMD ASPNETCORE_URLS=http://*:$PORT dotnet StripeDemoOKR.dll