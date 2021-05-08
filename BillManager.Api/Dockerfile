#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM node:latest AS node_base

RUN echo "NODE Version:" && node --version
RUN echo "NPM Version:" && npm --version



FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
COPY --from=node_base . .
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
COPY --from=node_base . .
WORKDIR /src
COPY ["BillManager.Api/BillManager.Api.csproj", "BillManager.Api/"]
COPY ["BillManager.Core/BillManager.Core.csproj", "BillManager.Core/"]
COPY ["BillManager.Core.Database/BillManager.Core.Database.csproj", "BillManager.Core.Database/"]
RUN dotnet restore "BillManager.Api/BillManager.Api.csproj"
COPY . .
WORKDIR "/src/BillManager.Api"
RUN dotnet build "BillManager.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BillManager.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "BillManager.Api.dll"]