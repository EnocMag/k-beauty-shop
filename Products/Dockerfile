FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

EXPOSE 80

# Copy everything
COPY *.slnx ./
COPY src/Products.Api/*.csproj ./src/Products.Api/
COPY src/Products.Domain/*.csproj ./src/Products.Domain/
COPY src/Products.Infrastructure/*.csproj ./src/Products.Infrastructure/

COPY tests/Products.Api.Tests/*.csproj ./tests/Products.Api.Tests/
COPY tests/Products.Domain.Tests/*.csproj ./tests/Products.Domain.Tests/
COPY tests/Products.Infrastructure.Tests/*.csproj ./tests/Products.Infrastructure.Tests/

# Restore as distinct layers
RUN dotnet restore

COPY . .
# Build and publish a release
RUN dotnet publish -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "Products.Api.dll"]