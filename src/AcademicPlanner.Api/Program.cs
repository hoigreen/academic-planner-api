using Microsoft.EntityFrameworkCore;
using AcademicPlanner.Api.Data;
using AcademicPlanner.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<IPrerequisiteEvaluator, PrerequisiteEvaluator>();
builder.Services.AddScoped<IStudentAuditService, StudentAuditService>();
builder.Services.AddScoped<IRoadmapRecommendationService, RoadmapRecommendationService>();
builder.Services.AddScoped<IPlanValidationService, PlanValidationService>();

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

app.MapControllers();

app.Run();
