FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend/WorkFlow/WorkFlow.csproj backend/WorkFlow/
RUN dotnet restore backend/WorkFlow/WorkFlow.csproj

COPY backend/WorkFlow/ backend/WorkFlow/
RUN dotnet publish backend/WorkFlow/WorkFlow.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .
RUN mkdir -p /app/uploads

EXPOSE 8080
ENTRYPOINT ["dotnet", "WorkFlow.dll"]
