FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PadelApi.Api/PadelApi.Api.csproj", "PadelApi.Api/"]
COPY ["PadelApi.Application/PadelApi.Application.csproj", "PadelApi.Application/"]
COPY ["PadelApi.Domain/PadelApi.Domain.csproj", "PadelApi.Domain/"]
COPY ["PadelApi.Infrastructure/PadelApi.Infrastructure.csproj", "PadelApi.Infrastructure/"]
RUN dotnet restore "PadelApi.Api/PadelApi.Api.csproj"
COPY . .
RUN dotnet publish "PadelApi.Api/PadelApi.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PadelApi.Api.dll"]
