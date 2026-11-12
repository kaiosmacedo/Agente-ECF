# Contexto de build: esta pasta, igual ao Root Directory do projeto no Vercel (ADR 0002).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG VERSAO=local
WORKDIR /src
COPY Api.RAG.csproj ./
RUN dotnet restore Api.RAG.csproj
COPY . ./
# O CD grava versao.txt, porque o build do Vercel não recebe o ARG (P19, REQ-EXEC-006).
RUN dotnet publish Api.RAG.csproj -c Release -o /app --no-restore -p:SourceRevisionId=$(cat versao.txt 2>/dev/null || echo "$VERSAO")

FROM mcr.microsoft.com/dotnet/aspnet:11.0
WORKDIR /app
COPY --from=build /app ./
USER $APP_UID
ENTRYPOINT ["dotnet", "Api.RAG.dll"]
