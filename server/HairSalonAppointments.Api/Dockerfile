FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files and restore
COPY ["HairSalonAppointments.Api/HairSalonAppointments.Api.csproj", "HairSalonAppointments.Api/"]
COPY ["HairSalonAppointments.Abstractions/HairSalonAppointments.Abstractions.csproj", "HairSalonAppointments.Abstractions/"]
COPY ["HairSalonAppointments.Contracts/HairSalonAppointments.Contracts.csproj", "HairSalonAppointments.Contracts/"]
COPY ["HairSalonAppointments.Infrastructure/HairSalonAppointments.Infrastructure.csproj", "HairSalonAppointments.Infrastructure/"]
COPY ["HairSalonAppointments.Providers/HairSalonAppointments.Providers.csproj", "HairSalonAppointments.Providers/"]

RUN dotnet restore "HairSalonAppointments.Api/HairSalonAppointments.Api.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/HairSalonAppointments.Api"
RUN dotnet build "./HairSalonAppointments.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./HairSalonAppointments.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create data directory for SQLite
RUN mkdir -p /app/data

ENV ASPNETCORE_URLS=http://+:8080
ENV ConnectionStrings__sqlite=Data Source=/app/data/hair.db

ENTRYPOINT ["dotnet", "HairSalonAppointments.Api.dll"]
