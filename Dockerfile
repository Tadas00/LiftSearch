# Learn about building .NET container images:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG TARGETARCH
WORKDIR /source

# copy csproj and restore as distinct layers
COPY source/LiftSearch/*.csproj .
RUN dotnet restore -a linus-musl-x64

# copy and publish app and libraries
COPY source/LiftSearch/. .
RUN dotnet publish --no-restore -a linus-musl-x64 -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["./LiftSearch"]

# Enable globalization and time zones:
ENV \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8

RUN apk add --no-cache \
    icu-data-full \
    icu-libs
