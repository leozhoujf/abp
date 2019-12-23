#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["MyCompanyName.MyProjectName.Web/MyCompanyName.MyProjectName.Web.csproj", "MyCompanyName.MyProjectName.Web/"]
RUN dotnet restore "MyCompanyName.MyProjectName.Web/MyCompanyName.MyProjectName.Web.csproj"
COPY . .
WORKDIR "/src/MyCompanyName.MyProjectName.Web"
RUN dotnet build "MyCompanyName.MyProjectName.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyCompanyName.MyProjectName.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyCompanyName.MyProjectName.Web.dll"]