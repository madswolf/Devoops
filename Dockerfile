# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY /aspnet/Minitwit/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY aspnet/Minitwit ./
RUN dotnet publish Minitwit.csproj  -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Minitwit.dll"]