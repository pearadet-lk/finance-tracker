# Patch: Add /health endpoint to Program.cs
#
# Add this BEFORE app.MapControllers() in Program.cs:
#
#   // ─── Health Check ────────────────────────────────────────────
#   builder.Services.AddHealthChecks()
#       .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!)
#       .AddRedis(builder.Configuration.GetConnectionString("Redis")!);
#
#   // then in the pipeline:
#   app.MapHealthChecks("/health");
#
# Also add to .csproj:
#   <PackageReference Include="AspNetCore.HealthChecks.NpgSql" Version="8.0.1" />
#   <PackageReference Include="AspNetCore.HealthChecks.Redis"  Version="8.0.1" />
#
# The k8s liveness/readiness probes call GET /health and expect HTTP 200.
