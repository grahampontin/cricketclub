# syntax=docker/dockerfile:1

# Multi-stage build for the CricketClub ASP.NET Core Web API

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution + project files first for better layer caching
COPY CricketClub.sln ./
COPY CricketClub.WebApi/CricketClub.WebApi.csproj CricketClub.WebApi/
COPY CricketClubDAL/CricketClubDAL/CricketClubDAL.csproj CricketClubDAL/CricketClubDAL/
COPY CricketClubDomain/CricketClubDomain.csproj CricketClubDomain/
COPY CricketClubMiddle/CricketClubMiddle.csproj CricketClubMiddle/
COPY CricketClubAccounts/CricketClubAccounts.csproj CricketClubAccounts/

# If there are additional projects referenced by the solution, they will be restored once sources are copied.
RUN dotnet restore CricketClub.WebApi/CricketClub.WebApi.csproj

# Copy the rest of the source
COPY . .

# Publish
RUN dotnet publish CricketClub.WebApi/CricketClub.WebApi.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Bind to port 8080 inside the container
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Copy published output
COPY --from=build /app/publish .

# Ensure log directory exists for log4net RollingFileAppender (relative path: logs/..)
RUN mkdir -p logs

ENTRYPOINT ["dotnet", "CricketClub.WebApi.dll"]

