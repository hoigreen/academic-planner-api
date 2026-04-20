using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace AcademicPlanner.Api.Services;

/// <summary>
/// Maps roles emitted by Keycloak into standard <see cref="ClaimTypes.Role"/>
/// claims so <c>[Authorize(Roles = "CVHT,Admin")]</c> and policy-based
/// authorization work transparently in ASP.NET Core.
///
/// Keycloak embeds roles into two places inside the access token:
///
///  • <c>realm_access.roles</c>                         — realm-wide roles
///  • <c>resource_access.&lt;clientId&gt;.roles</c>     — client-scoped roles
///
/// Both are extracted. The mapper also normalises a few Vietnamese / English
/// role aliases (e.g. <c>advisor</c> → <c>CVHT</c>, <c>student</c> → <c>SV</c>,
/// <c>admin</c> → <c>Admin</c>) to the application's canonical set.
/// </summary>
public static class KeycloakRoleMapper
{
    private const string RealmAccessClaim    = "realm_access";
    private const string ResourceAccessClaim = "resource_access";

    public static void MapKeycloakRoles(TokenValidatedContext context, string? clientId = null)
    {
        if (context.Principal?.Identity is not ClaimsIdentity identity) return;

        // 1. realm_access.roles
        var realmAccess = identity.FindFirst(RealmAccessClaim)?.Value;
        if (!string.IsNullOrWhiteSpace(realmAccess))
        {
            TryAddRolesFromJson(identity, realmAccess, "roles");
        }

        // 2. resource_access[clientId].roles
        if (!string.IsNullOrWhiteSpace(clientId))
        {
            var resourceAccess = identity.FindFirst(ResourceAccessClaim)?.Value;
            if (!string.IsNullOrWhiteSpace(resourceAccess))
            {
                try
                {
                    using var doc = JsonDocument.Parse(resourceAccess);
                    if (doc.RootElement.TryGetProperty(clientId, out var clientNode)
                        && clientNode.TryGetProperty("roles", out var clientRoles)
                        && clientRoles.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var role in clientRoles.EnumerateArray())
                        {
                            AddRoleClaim(identity, role.GetString());
                        }
                    }
                }
                catch (JsonException) { /* malformed resource_access — ignore */ }
            }
        }
    }

    private static void TryAddRolesFromJson(ClaimsIdentity identity, string json, string rolesProperty)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty(rolesProperty, out var roles)
                && roles.ValueKind == JsonValueKind.Array)
            {
                foreach (var role in roles.EnumerateArray())
                {
                    AddRoleClaim(identity, role.GetString());
                }
            }
        }
        catch (JsonException) { /* malformed JSON — ignore */ }
    }

    private static void AddRoleClaim(ClaimsIdentity identity, string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return;

        var normalised = Normalise(raw);

        // Avoid duplicates
        if (!identity.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == normalised))
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, normalised));
        }
    }

    /// <summary>
    /// Normalises a Keycloak role name into the application's canonical set
    /// (<c>Admin</c>, <c>CVHT</c>, <c>SV</c>). Unknown roles are passed
    /// through unchanged so realm-specific roles (e.g. Keycloak defaults like
    /// <c>offline_access</c>) remain available for fine-grained policies.
    /// </summary>
    private static string Normalise(string role) => role.Trim() switch
    {
        "admin"   or "Admin"   or "ADMIN"                        => "Admin",
        "advisor" or "Advisor" or "cvht" or "CVHT"               => "CVHT",
        "student" or "Student" or "sv"   or "SV"                 => "SV",
        _ => role
    };
}
