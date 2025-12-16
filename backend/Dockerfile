FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 複製整個 backend（包含 Application / Domain / Infrastructure）
COPY . .

# 指定正確的 csproj 路徑
RUN dotnet restore MyApp.Api/MyApp.Api.csproj
RUN dotnet publish MyApp.Api/MyApp.Api.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "MyApp.Api.dll"]
