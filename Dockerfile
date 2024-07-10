# https://learn.microsoft.com/en-us/dotnet/core/docker/build-container?tabs=windows
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

ARG GITHUB_BUILD_NUMBER
ARG GITHUB_BRANCH_REF
ARG GITHUB_COMMIT

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish WookiepediaStatusArticleData/WookiepediaStatusArticleData.csproj --output out -p:BuildNumber=$GITHUB_BUILD_NUMBER -p:Branch=$GITHUB_BRANCH_REF -p:Commit=$GITHUB_COMMIT

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "WookiepediaStatusArticleData.dll"]