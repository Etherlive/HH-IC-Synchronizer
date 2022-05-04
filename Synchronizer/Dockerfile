#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Synchronizer/Synchronizer.csproj", "Synchronizer/"]
COPY ["ICompleat-Interface-Library/ICompleat/ICompleat.csproj", "ICompleat-Interface-Library/ICompleat/"]
COPY ["ICompleat-Interface-Library/API-Whisperer/API-Whisperer/API-Whisperer.csproj", "ICompleat-Interface-Library/API-Whisperer/API-Whisperer/"]
COPY ["Hire-Hop-Interface-Library/Hire-Hop-Interface/Hire-Hop-Interface.csproj", "Hire-Hop-Interface-Library/Hire-Hop-Interface/"]
RUN dotnet restore "Synchronizer/Synchronizer.csproj"
COPY . .
WORKDIR "/src/Synchronizer"
RUN dotnet build "Synchronizer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Synchronizer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Synchronizer.dll"]