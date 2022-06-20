FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR "/src"
ENV NUGET_PACKAGES=/devops/.nuget/packages
RUN groupadd --gid 1200 devops \
	&& useradd --home-dir /devops --create-home --uid 1200 --gid 1200 devops \
	&& mkdir /devops/.nuget \
	&& chown -R devops:devops /src \
	&& chown -R devops:devops /devops

COPY ./RTGS.IDCrypt.Service.sln .
COPY ./*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*}/ && mv $file ${file%.*}/; done
RUN dotnet restore RTGS.IDCrypt.Service.sln

WORKDIR "/src"
COPY --chown=devops:devops . .
RUN dotnet build -c Release --no-restore
RUN dotnet test -c Release -o /app/test --no-restore "./RTGS.IDCrypt.Service.Scheduler.Tests/RTGS.IDCrypt.Service.Scheduler.Tests.csproj"; \
    exit_code=$?; \
    dotnet test -c Release -o /app/test --no-restore "./RTGS.IDCrypt.Service.Scheduler.IntegrationTests/RTGS.IDCrypt.Service.Scheduler.IntegrationTests.csproj"; \
    exit $(( $exit_code || $? ))
RUN chown -R devops:devops /src/RTGS.IDCrypt.Service.Scheduler/obj/ \
		&& chown -R devops:devops /src/RTGS.IDCrypt.Service.Scheduler/bin/

FROM build AS publish
WORKDIR "/src"
RUN dotnet publish "./RTGS.IDCrypt.Service.Scheduler/RTGS.IDCrypt.Service.Scheduler.csproj" --no-restore -c Release -o /devops/publish

FROM base AS final
WORKDIR "/app"
COPY --from=publish /devops/publish .
ENTRYPOINT ["RTGS.IDCrypt.Service.Scheduler"]	
