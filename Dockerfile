# Use the official .NET SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY HoneyWebPlatform.Web/*.csproj ./HoneyWebPlatform.Web/
COPY HoneyWebPlatform.Data/*.csproj ./HoneyWebPlatform.Data/
COPY HoneyWebPlatform.Data.Models/*.csproj ./HoneyWebPlatform.Data.Models/
COPY HoneyWebPlatform.Services.Data/*.csproj ./HoneyWebPlatform.Services.Data/
COPY HoneyWebPlatform.Services.Data.Models/*.csproj ./HoneyWebPlatform.Services.Data.Models/
COPY HoneyWebPlatform.Services.Mapping/*.csproj ./HoneyWebPlatform.Services.Mapping/
COPY HoneyWebPlatform.Web.Infrastructure/*.csproj ./HoneyWebPlatform.Web.Infrastructure/
COPY HoneyWebPlatform.Web.ViewModels/*.csproj ./HoneyWebPlatform.Web.ViewModels/
RUN dotnet restore ./HoneyWebPlatform.Web/HoneyWebPlatform.Web.csproj

# Copy everything else and build
COPY . .
WORKDIR /app/HoneyWebPlatform.Web
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/HoneyWebPlatform.Web/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "HoneyWebPlatform.Web.dll"] 