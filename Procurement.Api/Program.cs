using Procurement.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Serilog;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

//builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddProcurementPersistence(builder.Configuration);

// Register infrastructure services (single extension method)
builder.Services.AddProcurementInfrastructure();

// Add MediatR for Application layer
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Procurement.Application.Class1>());

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetSection("Database")["ConnectionString"]!);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
//app.UseSerilogRequestLogging();

app.MapHealthChecks("/healthz");

// Apply migrations in dev (optional)
//if (app.Environment.IsDevelopment())
//{
//    using var scope = app.Services.CreateScope();
//    // Replace with your ProcurementDbContext when implemented
//    // var db = scope.ServiceProvider.GetRequiredService<ProcurementDbContext>();
//    // await db.Database.MigrateAsync();
//}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
