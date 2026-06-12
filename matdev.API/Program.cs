using matdev.API.Extensions;
using matdev.API.Helpers;
using matdev.Application.Interfaces;
using matdev.Application.Mapping;
using matdev.Application.Services;
using matdev.Domain.Interfaces;
using matdev.Infrastructure.Data;
using matdev.Infrastructure.Data.Seeds;
using matdev.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<matdev.API.ExceptionHandling.GlobalExceptionHandler>();


// Register dependencies and PostgreSQL (see ConnectionStrings:Database)
builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddAutoMapper(
    cfg => cfg.AddProfile<ApplicationMappingProfile>());

builder.Services.AddItemServices();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await DbMigrate.MigrateDbAsync(app);

using (var scope = app.Services.CreateScope())
{
    await SeedData.SeedAsync(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors();

app.UseAuthorization();

app.MapGet("/api/health", () => Results.Ok(new { utc = DateTime.UtcNow.ToString("o") }))
    .WithName("Health")
    .WithTags("Health");

app.MapControllers();

app.Run();
