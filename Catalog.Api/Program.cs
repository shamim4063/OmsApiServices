using BuildingBlocks.Web.Errors;
using Catalog.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
//builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddCatalogPersistence(builder.Configuration);

// Register infrastructure services (single extension method)
builder.Services.AddCatalogInfrastructure();

// Add MediatR for Application layer
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Catalog.Application.Products.CreateProduct>());

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetSection("Database")["ConnectionString"]!);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseProblemDetails(); // <- Our custom ProblemDetails(Error) middleware
//app.UseSerilogRequestLogging();

app.MapHealthChecks("/healthz");

// Apply migrations in dev (optional)
// if (app.Environment.IsDevelopment())
// {
//     using var scope = app.Services.CreateScope();
//     var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
//     await db.Database.MigrateAsync();
// }

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();


app.Run();

