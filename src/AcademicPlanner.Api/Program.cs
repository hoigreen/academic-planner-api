using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using AcademicPlanner.Api.Data;
using AcademicPlanner.Api.Models;
using AcademicPlanner.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ──────────────────────────────────────────────────────────────
// Authentication: Keycloak is the sole Identity Provider.
// Supported application roles: Admin, CVHT, SV.
// ──────────────────────────────────────────────────────────────
var keycloakSection  = builder.Configuration.GetSection("Keycloak");
var keycloakAuthority = keycloakSection["Authority"];   // e.g. https://auth.example.com/realms/academic-planner
var keycloakAudience  = keycloakSection["Audience"];    // e.g. academic-planner-api
var keycloakClientId  = keycloakSection["ClientId"];    // e.g. academic-planner-web (used for resource_access mapping)
var validateAudience  = keycloakSection.GetValue<bool?>("ValidateAudience") ?? !string.IsNullOrWhiteSpace(keycloakAudience);

var requireAuth = !string.IsNullOrWhiteSpace(keycloakAuthority);

if (requireAuth)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = keycloakAuthority;
            options.Audience = keycloakAudience;
            options.RequireHttpsMetadata = builder.Environment.IsProduction();
            options.MapInboundClaims = false; // keep original Keycloak claim names

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = keycloakAuthority,
                ValidateAudience = validateAudience,
                ValidAudience = keycloakAudience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                NameClaimType = "preferred_username",
                RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    KeycloakRoleMapper.MapKeycloakRoles(context, keycloakClientId);
                    return Task.CompletedTask;
                }
            };
        });

    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("RequireAdvisor", policy => policy.RequireRole("CVHT", "Admin"))
        .AddPolicy("RequireStudent", policy => policy.RequireRole("SV", "CVHT", "Admin"))
        .AddPolicy("RequireAdmin",   policy => policy.RequireRole("Admin"));
}
else
{
    // Dev mode (no Keycloak configured): all policies pass-through so
    // developers can hit the API without booting an IdP.
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = false,
            };
        });

    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("RequireAdvisor", policy => policy.RequireAssertion(_ => true))
        .AddPolicy("RequireStudent", policy => policy.RequireAssertion(_ => true))
        .AddPolicy("RequireAdmin",   policy => policy.RequireAssertion(_ => true));
}

// ──────────────────────────────────────────────────────────────
// PostgreSQL / EF Core — register ORDBMS composite type
// ──────────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration.GetConnectionString("Default");

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
// Register the acad.knowledge_block composite type for ORDBMS array support
dataSourceBuilder.MapComposite<KnowledgeBlock>("acad.knowledge_block");
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dataSource));

// ──────────────────────────────────────────────────────────────
// Application services
// ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IPrerequisiteEvaluator, PrerequisiteEvaluator>();
builder.Services.AddScoped<IStudentAuditService, StudentAuditService>();
builder.Services.AddScoped<IRoadmapRecommendationService, RoadmapRecommendationService>();
builder.Services.AddScoped<IPlanValidationService, PlanValidationService>();
builder.Services.AddScoped<IExcelImportService, ExcelImportService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("default", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});

var app = builder.Build();

app.UseCors("default");
app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();

if (requireAuth)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapControllers();

app.Run();
