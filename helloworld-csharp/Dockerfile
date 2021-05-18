# Use Microsoft's official build .NET image.
# https://hub.docker.com/_/microsoft-dotnet-core-sdk/
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
#FROM mcr.microsoft.com/dotnet/core/sdk:5.0.100-preview-alpine AS build
WORKDIR /app

# Install production dependencies.
# Copy csproj and restore as distinct layers.
COPY *.csproj ./
RUN dotnet restore

# Install a lint checker
RUN dotnet tool update -g dotnet-format

# Copy local code to the container image.
COPY . ./
WORKDIR /app

# Run linting before building Release artifact
RUN ~/.dotnet/tools/dotnet-format --check --verbosity diagnostic

# Run a test before building Release artifact
RUN dotnet test

# Build a release artifact.
RUN dotnet publish -c Release -o out


# Use Microsoft's official runtime .NET image.
# https://hub.docker.com/_/microsoft-dotnet-core-aspnet/
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS runtime
WORKDIR /app
COPY --from=build /app/out ./

ENV PORT=8080 TARGET=cloud
EXPOSE 8080
# Run the web service on container startup.
ENTRYPOINT ["dotnet", "helloworld-csharp.dll"]
