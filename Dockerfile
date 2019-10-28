FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS base
WORKDIR /app
EXPOSE 88

FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /src
COPY ["vscodecore.csproj", "./"]
RUN dotnet restore "./vscodecore.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "vscodecore.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "vscodecore.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "vscodecore.dll"]
