FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY NetAuth.sln ./
COPY src/NetAuth/NetAuth.csproj src/NetAuth/
RUN dotnet restore
COPY src/NetAuth/ src/NetAuth/
WORKDIR /src/src/NetAuth
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NetAuth.dll"]
