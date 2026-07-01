FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/DateMatcher.Web/DateMatcher.Web.csproj", "DateMatcher.Web/"]
COPY ["src/DateMatcher.Application/DateMatcher.Application.csproj", "DateMatcher.Application/"]
COPY ["src/DateMatcher.Infrastructure/DateMatcher.Infrastructure.csproj", "DateMatcher.Infrastructure/"]
COPY ["src/DateMatcher.Domain/DateMatcher.Domain.csproj", "DateMatcher.Domain/"]

RUN dotnet restore "DateMatcher.Web/DateMatcher.Web.csproj"

COPY src/ .
WORKDIR /src/DateMatcher.Web
RUN dotnet publish "DateMatcher.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV DatabaseOptions__DefaultConnection="Data Source=/app/data/DateMatcher.db"

EXPOSE 8080

COPY --from=build /app/publish .
RUN mkdir -p /app/data

VOLUME ["/app/data"]

ENTRYPOINT ["dotnet", "DateMatcher.Web.dll"]
