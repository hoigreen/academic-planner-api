FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY academic-planner-api.generated.sln ./
COPY src/AcademicPlanner.Api/AcademicPlanner.Api.csproj src/AcademicPlanner.Api/
RUN dotnet restore academic-planner-api.generated.sln

COPY . .
RUN dotnet publish src/AcademicPlanner.Api/AcademicPlanner.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "AcademicPlanner.Api.dll"]
