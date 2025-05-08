# Etapa base
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000

# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ./OpenAiApiDemo/OpenAiApiDemo.csproj ./OpenAiApiDemo/
RUN dotnet restore ./OpenAiApiDemo/OpenAiApiDemo.csproj

COPY ./OpenAiApiDemo ./OpenAiApiDemo
WORKDIR /src/OpenAiApiDemo
RUN dotnet publish -c Release -o /app/publish

# Etapa final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OpenAiApiDemo.dll"]
