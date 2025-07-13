FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

RUN adduser --system --uid 1000 appuser

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY SkillLearning.sln ./
COPY Backend/*.csproj Backend/
COPY Backend/ Backend/

RUN dotnet restore SkillLearning.sln --runtime linux-x64

WORKDIR /src/Backend/SkillLearning.Api
RUN dotnet build SkillLearning.Api.csproj -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish SkillLearning.Api.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER appuser
ENTRYPOINT ["dotnet", "SkillLearning.Api.dll"]