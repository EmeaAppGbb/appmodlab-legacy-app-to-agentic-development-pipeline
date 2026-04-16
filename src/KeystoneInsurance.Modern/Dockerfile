# ---------------------------------------------------------------------------
# Keystone Insurance – Multi-stage Dockerfile
# Target: .NET 10 (matches csproj TargetFramework net10.0)
# ---------------------------------------------------------------------------

# ── Stage 1: Build ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy project file first for layer caching
COPY KeystoneInsurance.Modern.csproj ./
RUN dotnet restore

# Copy remaining source and publish
COPY . ./
RUN dotnet publish -c Release -o /app/publish --no-restore \
    /p:UseAppHost=false

# ── Stage 2: Runtime ────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

# Security: run as non-root
RUN groupadd --gid 1654 appgroup && \
    useradd --uid 1654 --gid appgroup --shell /bin/false --create-home appuser

# Install ca-certificates for HTTPS and set invariant globalization
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

COPY --from=build /app/publish .

# Expose the port Container Apps expects
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

USER appuser

HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
    CMD ["dotnet", "KeystoneInsurance.Modern.dll", "--urls", "http://localhost:8080/health/live"]

ENTRYPOINT ["dotnet", "KeystoneInsurance.Modern.dll"]
