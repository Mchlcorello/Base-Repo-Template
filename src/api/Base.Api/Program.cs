using System.Reflection;
using Base.Api.Data;
using Base.Api.Health;
using Base.Api.Infrastructure;
using Base.Api.Observability;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

const string LocalReactCors = "LocalReact";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAppServices(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy(LocalReactCors, policy => policy
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Base.Api", Version = "v1" });
});

builder.Services.AddProblemDetails();

builder.Services.AddAppHealthChecks();

builder.Services.AddAppObservability(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Detail = app.Environment.IsDevelopment() ? feature?.Error.Message : null,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = problem.Status.Value;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(LocalReactCors);

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DevelopmentSeeder.SeedAsync(db);
}

var serviceName = builder.Configuration["Observability:ServiceName"] ?? "Base.Api";
var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";

app.MapGet("/api/info", () => Results.Ok(new
{
    service = serviceName,
    version,
    environment = app.Environment.EnvironmentName
}))
.WithName("GetInfo")
.WithTags("Info");

app.MapAppHealthChecks();

app.MapAppObservability();

app.Run();
