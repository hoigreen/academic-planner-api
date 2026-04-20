using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace AcademicPlanner.Api.Services;

public static class KeycloakRoleMapper
{
    public static void MapKeycloakRoles(TokenValidatedContext context)
    {
        var identity = context.Principal?.Identity as ClaimsIdentity;
        if (identity is null) return;

        var realmAccess = context.Principal?.FindFirst("realm_access")?.Value;
        if (string.IsNullOrWhiteSpace(realmAccess)) return;

        try
        {
            using var doc = JsonDocument.Parse(realmAccess);
            if (doc.RootElement.TryGetProperty("roles", out var roles) && roles.ValueKind == JsonValueKind.Array)
            {
                foreach (var role in roles.EnumerateArray())
                {
                    var roleName = role.GetString();
                    if (!string.IsNullOrWhiteSpace(roleName))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                    }
                }
            }
        }
        catch (JsonException)
        {
            // Malformed realm_access claim — skip role mapping
        }
    }
}
