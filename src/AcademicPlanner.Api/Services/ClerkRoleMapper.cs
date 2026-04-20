using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace AcademicPlanner.Api.Services;

/// <summary>
/// Extracts Clerk role(s) from the JWT <c>public_metadata</c> claim and adds them
/// as standard <c>ClaimTypes.Role</c> so ASP.NET Core policy authorization works.
///
/// Clerk embeds custom metadata into the JWT when you configure a "role" key inside
/// publicMetadata on the user object. The claim key is <c>public_metadata</c> and the
/// value is a JSON object, e.g. {"role":"advisor"} or {"role":["advisor","admin"]}.
/// </summary>
public static class ClerkRoleMapper
{
    private const string PublicMetadataClaim = "public_metadata";
    private const string OrgRoleClaim = "org_role";

    public static void MapClerkRoles(TokenValidatedContext context)
    {
        var identity = context.Principal?.Identity as ClaimsIdentity;
        if (identity is null) return;

        // 1. Try public_metadata.role (custom roles set via Clerk dashboard or API)
        var metadataClaim = identity.FindFirst(PublicMetadataClaim);
        if (metadataClaim is not null)
        {
            try
            {
                using var doc = JsonDocument.Parse(metadataClaim.Value);
                if (doc.RootElement.TryGetProperty("role", out var roleElement))
                {
                    AddRoleClaims(identity, roleElement);
                }
            }
            catch (JsonException) { /* ignore malformed metadata */ }
        }

        // 2. Try org_role (Clerk organizations — maps "org:admin" → "Admin", etc.)
        var orgRoleClaim = identity.FindFirst(OrgRoleClaim);
        if (orgRoleClaim is not null)
        {
            var mapped = orgRoleClaim.Value switch
            {
                "org:admin" => "Admin",
                "org:advisor" or "org:CVHT" => "CVHT",
                "org:student" or "org:SV" => "SV",
                _ => orgRoleClaim.Value
            };
            identity.AddClaim(new Claim(ClaimTypes.Role, mapped));
        }
    }

    private static void AddRoleClaims(ClaimsIdentity identity, JsonElement roleElement)
    {
        if (roleElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in roleElement.EnumerateArray())
            {
                var r = item.GetString();
                if (!string.IsNullOrWhiteSpace(r))
                    identity.AddClaim(new Claim(ClaimTypes.Role, r));
            }
        }
        else if (roleElement.ValueKind == JsonValueKind.String)
        {
            var r = roleElement.GetString();
            if (!string.IsNullOrWhiteSpace(r))
                identity.AddClaim(new Claim(ClaimTypes.Role, r));
        }
    }
}
