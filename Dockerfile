# ========== BUILD AŞAMASI ==========
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Proje dosyalarını kopyala (restore için)
COPY tarimpazari.slnx ./
COPY TarimPazari.Core/TarimPazari.Core.csproj TarimPazari.Core/
COPY TarimPazari.DataAccess/TarimPazari.DataAccess.csproj TarimPazari.DataAccess/
COPY TarimPazari.Business/TarimPazari.Business.csproj TarimPazari.Business/
COPY tarimpazari/tarimpazari.csproj tarimpazari/

# NuGet paketlerini restore et
RUN dotnet restore tarimpazari/tarimpazari.csproj

# Tüm kaynak kodunu kopyala
COPY . .

# Uygulamayı derle
RUN dotnet publish tarimpazari/tarimpazari.csproj -c Release -o /app/publish --no-restore

# ========== RUNTIME AŞAMASI ==========
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Publish edilen dosyaları kopyala
COPY --from=build /app/publish .

EXPOSE 10000

ENTRYPOINT ["dotnet", "tarimpazari.dll"]
