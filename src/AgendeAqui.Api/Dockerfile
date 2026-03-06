FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/AgendeAqui.Domain/AgendeAqui.Domain.csproj src/AgendeAqui.Domain/
COPY src/AgendeAqui.Application/AgendeAqui.Application.csproj src/AgendeAqui.Application/
COPY src/AgendeAqui.Infrastructure/AgendeAqui.Infrastructure.csproj src/AgendeAqui.Infrastructure/
COPY src/AgendeAqui.Api/AgendeAqui.Api.csproj src/AgendeAqui.Api/

RUN dotnet restore src/AgendeAqui.Api/AgendeAqui.Api.csproj

COPY src/ src/

RUN dotnet publish src/AgendeAqui.Api/AgendeAqui.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN addgroup --system --gid 1001 appgroup && \
    adduser --system --uid 1001 --ingroup appgroup appuser

COPY --from=build /app/publish .

USER appuser

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AgendeAqui.Api.dll"]
