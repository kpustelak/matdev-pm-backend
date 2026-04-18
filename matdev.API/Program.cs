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


// Register dependencies And Use InMemory Database
builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddAutoMapper(
    cfg => cfg.AddProfile<ApplicationMappingProfile>());

builder.Services.AddItemServices();

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
