FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR "/src"

COPY ./RTGS.IDCrypt.Service.sln .
COPY ./*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*}/ && mv $file ${file%.*}/; done
RUN dotnet restore RTGS.IDCrypt.Service.sln

COPY . .
RUN dotnet build -c Release --no-restore
RUN dotnet test -c Release --no-restore "./RTGS.IDCrypt.Service.Tests/RTGS.IDCrypt.Service.Tests.csproj"; \
    exit_code=$?; \
    dotnet test -c Release --no-restore "./RTGS.IDCrypt.Service.IntegrationTests/RTGS.IDCrypt.Service.IntegrationTests.csproj"; \
    exit $(( $exit_code || $? ))

FROM build AS publish
WORKDIR "/src"
RUN dotnet publish "./RTGS.IDCrypt.Service/RTGS.IDCrypt.Service.csproj" --no-restore -c Release -o /devops/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
EXPOSE 80
EXPOSE 443
WORKDIR "/app"
COPY --from=publish /devops/publish .
ENTRYPOINT ["dotnet", "RTGS.IDCrypt.Service.dll"]
