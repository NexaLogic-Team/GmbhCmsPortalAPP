# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the project file first to restore dependencies efficiently
COPY ["BlazorCMSDotNetTenWithTailwind.csproj", "./"]
RUN dotnet restore "BlazorCMSDotNetTenWithTailwind.csproj"

# Copy everything else and build
COPY . .
RUN dotnet publish "BlazorCMSDotNetTenWithTailwind.csproj" -c Release -o /app/publish

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BlazorCMSDotNetTenWithTailwind.dll"]