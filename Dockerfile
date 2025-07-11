FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY SkillLearning.sln ./
COPY Backend/SkillLearning.Api Backend/SkillLearning.Api
COPY Backend/SkillLearning.Application Backend/SkillLearning.Application
COPY Backend/SkillLearning.Domain Backend/SkillLearning.Domain
COPY Backend/SkillLearning.Infrastructure Backend/SkillLearning.Infrastructure
COPY Backend/SkillLearning.Workers.EmailSender Backend/SkillLearning.Workers.EmailSender
COPY Backend/SkillLearning.Tests Backend/SkillLearning.Tests

RUN dotnet restore SkillLearning.sln

WORKDIR /src/Backend/SkillLearning.Api
RUN dotnet build SkillLearning.Api.csproj -c $BUILD_CONFIGURATION --no-restore

FROM build AS publish
RUN dotnet publish SkillLearning.Api.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SkillLearning.Api.dll"]