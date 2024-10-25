# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG DEFAULT_CONNECTION
ARG SQL_SERVER_VERSION
WORKDIR /src
COPY ["Quizer.csproj", "."]
RUN dotnet restore "./Quizer.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./Quizer.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet tool install --global dotnet-ef --version 3.1
ENV PATH="$PATH:/root/.dotnet/tools"
ENV ConnectionStrings__DefaultConnection=$DEFAULT_CONNECTION
ENV SqlServerVersion=$SQL_SERVER_VERSION
#RUN dotnet tool restore
#RUN dotnet ef migrations add InitialCreate

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Quizer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Quizer.dll"]