using Customers.Behaviors;
using Customers.Infrastructure.DbContexts;
using Customers.Utils;
using FastEndpoints.Swagger;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));

// FastEndpoints & Swagger
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument(o =>
{
    o.DocumentSettings = s =>
    {
        s.Title = "Customers API";
        s.Version = "v1";
        s.Description = "Customers microservice with Vertical Slice Architecture, FastEndpoints, MediatR, and MongoDB EF Core";
    };
});

// MediatR & Validation Pipeline
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly));

// MongoDB EF Core
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDb")
    ?? "mongodb://localhost:27017";
var mongoDatabaseName = builder.Configuration.GetValue<string>("DatabaseName")
    ?? "CustomersDb";

builder.Services.AddDbContext<CustomersDbContext>(options =>
    options.UseMongoDB(mongoConnectionString, mongoDatabaseName));

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<CustomersDbContext>(name: "mongo-customers-db", tags: ["ready"]);

// Exception handling & ProblemDetails
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ProblemsExceptionHandler>();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseFastEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                durationMs = entry.Value.Duration.TotalMilliseconds,
                error = entry.Value.Exception?.Message
            })
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});

app.Run();
