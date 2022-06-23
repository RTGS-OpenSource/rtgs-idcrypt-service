FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR "/src"

COPY ./RTGS.IDCrypt.Service.sln .
COPY ./*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*}/ && mv $file ${file%.*}/; done
RUN dotnet restore RTGS.IDCrypt.Service.sln

COPY . .
RUN dotnet build -c Release --no-restore
RUN dotnet test -c Release --no-restore "./RTGS.IDCrypt.Service.ConnectionCleanupScheduler.Tests/RTGS.IDCrypt.Service.ConnectionCleanupScheduler.Tests.csproj"; \
    exit_code=$?; \
    dotnet test -c Release --no-restore "./RTGS.IDCrypt.Service.ConnectionCleanupScheduler.IntegrationTests/RTGS.IDCrypt.Service.ConnectionCleanupScheduler.IntegrationTests.csproj"; \
    exit $(( $exit_code || $? ))

FROM build AS publish
WORKDIR "/src"
RUN dotnet publish "./RTGS.IDCrypt.Service.ConnectionCleanupScheduler/RTGS.IDCrypt.Service.ConnectionCleanupScheduler.csproj" --no-restore -c Release -o /devops/publish

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS final
WORKDIR "/app"
COPY --from=publish /devops/publish .
ENTRYPOINT ["./RTGS.IDCrypt.Service.ConnectionCleanupScheduler"]
