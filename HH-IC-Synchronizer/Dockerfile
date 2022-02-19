FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["HH-IC-Synchronizer/HH-IC-Synchronizer.csproj", "HH-IC-Synchronizer/"]
COPY ["Hire-Hop-Interface-Library/Hire-Hop-Interface/Hire-Hop-Interface.csproj", "Hire-Hop-Interface-Library/Hire-Hop-Interface/"]
COPY ["ICompleat-Interface-Library/ICompleat/ICompleat.csproj", "ICompleat-Interface-Library/ICompleat/"]
RUN dotnet restore "HH-IC-Synchronizer/HH-IC-Synchronizer.csproj"
COPY . .
WORKDIR "/src/HH-IC-Synchronizer"
RUN dotnet build "HH-IC-Synchronizer.csproj" -c Release -o /app/build

FROM build AS publish
RUN apt-get update -yq \
    && apt-get install curl gnupg -yq \
    && curl -sL https://deb.nodesource.com/setup_14.x | bash \
    && apt-get install nodejs -yq
RUN dotnet publish "HH-IC-Synchronizer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HH-IC-Synchronizer.dll"]
