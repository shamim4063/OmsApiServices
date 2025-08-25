var builder = WebApplication.CreateBuilder(args);

// Serilog (optional)
//builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

// YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// CORS (optional - tighten for your frontends)
builder.Services.AddCors(o => o.AddPolicy("default", p =>
    p.WithOrigins("http://localhost:4200", "http://localhost:3000")
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()));

//builder.Services.AddAuthentication("Bearer")
//    .AddJwtBearer("Bearer", o =>
//    {
//        o.Authority = "https://your-idp"; // e.g., Auth0/Keycloak
//        o.Audience = "erp-api";           // API resource
//        o.RequireHttpsMetadata = true;
//        // o.TokenValidationParameters = new TokenValidationParameters { ... };
//    });

// builder.Services.AddAuthorization();

var app = builder.Build();

//app.UseSerilogRequestLogging();
app.UseCors("default");

// Auth (see section 5) goes before proxy
//app.UseAuthentication();
//app.UseAuthorization();

// Protect all proxied routes (or per-route via metadata)
// app.MapReverseProxy().RequireAuthorization();

//app.UseRateLimiter();

// Basic health
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

// Forward X-Request-ID / correlation
app.Use(async (ctx, next) =>
{
    var cid = ctx.Request.Headers["x-correlation-id"];
    if (string.IsNullOrEmpty(cid))
    {
        cid = Guid.NewGuid().ToString("n");
        ctx.Request.Headers["x-correlation-id"] = cid!;
    }
    ctx.Response.Headers["x-correlation-id"] = cid!;
    await next();
});

// YARP proxy
app.MapReverseProxy();

app.Run();
