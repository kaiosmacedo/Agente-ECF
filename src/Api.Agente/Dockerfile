# Contexto de build: esta pasta, igual ao Root Directory do projeto no Vercel (ADR 0002).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG VERSAO=local
WORKDIR /src
COPY Api.Agente.csproj ./
RUN dotnet restore Api.Agente.csproj
COPY . ./
RUN dotnet publish Api.Agente.csproj -c Release -o /app --no-restore -p:SourceRevisionId=$VERSAO

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app ./
USER $APP_UID
ENTRYPOINT ["dotnet", "Api.Agente.dll"]
