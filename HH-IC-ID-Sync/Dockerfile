#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["HH-IC-ID-Sync/HH-IC-ID-Sync.csproj", "HH-IC-ID-Sync/"]
COPY ["ICompleat-Interface-Library/ICompleat/ICompleat.csproj", "ICompleat-Interface-Library/ICompleat/"]
COPY ["ICompleat-Interface-Library/API-Whisperer/API-Whisperer/API-Whisperer.csproj", "ICompleat-Interface-Library/API-Whisperer/API-Whisperer/"]
COPY ["Hire-Hop-Interface-Library/Hire-Hop-Interface/Hire-Hop-Interface.csproj", "Hire-Hop-Interface-Library/Hire-Hop-Interface/"]
RUN dotnet restore "HH-IC-ID-Sync/HH-IC-ID-Sync.csproj"
COPY . .
WORKDIR "/src/HH-IC-ID-Sync"
RUN dotnet build "HH-IC-ID-Sync.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HH-IC-ID-Sync.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HH-IC-ID-Sync.dll"]