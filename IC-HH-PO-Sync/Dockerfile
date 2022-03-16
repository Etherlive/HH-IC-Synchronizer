#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["IC-HH-PO-Sync/IC-HH-PO-Sync.csproj", "IC-HH-PO-Sync/"]
COPY ["ICompleat-Interface-Library/ICompleat/ICompleat.csproj", "ICompleat-Interface-Library/ICompleat/"]
COPY ["ICompleat-Interface-Library/API-Whisperer/API-Whisperer/API-Whisperer.csproj", "ICompleat-Interface-Library/API-Whisperer/API-Whisperer/"]
COPY ["Hire-Hop-Interface-Library/Hire-Hop-Interface/Hire-Hop-Interface.csproj", "Hire-Hop-Interface-Library/Hire-Hop-Interface/"]
RUN dotnet restore "IC-HH-PO-Sync/IC-HH-PO-Sync.csproj"
COPY . .
WORKDIR "/src/IC-HH-PO-Sync"
RUN dotnet build "IC-HH-PO-Sync.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IC-HH-PO-Sync.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IC-HH-PO-Sync.dll"]