#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["AspNetCore.Identity.CosmosDb.Example/AspNetCore.Identity.CosmosDb.Example.csproj", "AspNetCore.Identity.CosmosDb.Example/"]
RUN dotnet restore "AspNetCore.Identity.CosmosDb.Example/AspNetCore.Identity.CosmosDb.Example.csproj"
COPY . .
WORKDIR "/src/AspNetCore.Identity.CosmosDb.Example"
RUN dotnet build "AspNetCore.Identity.CosmosDb.Example.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AspNetCore.Identity.CosmosDb.Example.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AspNetCore.Identity.CosmosDb.Example.dll"]