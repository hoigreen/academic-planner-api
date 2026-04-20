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
// Authentication: supports Clerk JWT (primary) or Keycloak JWT
// ──────────────────────────────────────────────────────────────
var clerkSection = builder.Configuration.GetSection("Clerk");
var clerkAuthority = clerkSection["Authority"];          // e.g. https://<instance>.clerk.accounts.dev
var clerkAudience = clerkSection["Audience"];

var keycloakSection = builder.Configuration.GetSection("Keycloak");
var keycloakAuthority = keycloakSection["Authority"];

// Resolve which authority to use (Clerk takes priority)
var jwtAuthority = !string.IsNullOrWhiteSpace(clerkAuthority) ? clerkAuthority
    : !string.IsNullOrWhiteSpace(keycloakAuthority) ? keycloakAuthority
    : null;

var requireAuth = !string.IsNullOrWhiteSpace(jwtAuthority);

if (requireAuth)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = jwtAuthority;
            options.Audience = clerkAudience ?? "academic-planner";
            options.RequireHttpsMetadata = builder.Environment.IsProduction();
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = !string.IsNullOrWhiteSpace(clerkAudience ?? keycloakSection["Audience"]),
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                NameClaimType = "sub",
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    // Extract roles from Clerk metadata or Keycloak realm_access
                    if (!string.IsNullOrWhiteSpace(clerkAuthority))
                        ClerkRoleMapper.MapClerkRoles(context);
                    else
                        KeycloakRoleMapper.MapKeycloakRoles(context);
                    return Task.CompletedTask;
                }
            };
        });

    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("RequireAdvisor", policy => policy.RequireRole("CVHT", "advisor", "Admin"))
        .AddPolicy("RequireStudent",  policy => policy.RequireRole("SV", "student", "CVHT", "advisor", "Admin"))
        .AddPolicy("RequireAdmin",    policy => policy.RequireRole("Admin", "admin"));
}
else
{
    // Dev mode: all policies pass-through
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
        .AddPolicy("RequireStudent",  policy => policy.RequireAssertion(_ => true))
        .AddPolicy("RequireAdmin",    policy => policy.RequireAssertion(_ => true));
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
