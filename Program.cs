using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Text;
using Unilife.Data;
using Unilife.Models;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Redis ─────────────────────────────────────────────────
var redisUrl    = builder.Configuration["Redis:Url"]!;
var redisConfig = ConfigurationOptions.Parse(redisUrl);
redisConfig.AbortOnConnectFail = false;
var redis = ConnectionMultiplexer.Connect(redisConfig);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

// Cache distribuido en Redis (para datos de app)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.ConfigurationOptions = redisConfig;
    options.InstanceName         = "UniLife:Cache:";
});

// Data Protection con llaves persistidas en Redis
builder.Services.AddDataProtection()
    .PersistKeysToStackExchangeRedis(redis, "UniLife:DataProtection:Keys")
    .SetApplicationName("UniLife");

// ── Identity ──────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(10);
    options.Lockout.AllowedForNewUsers      = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ── Cookie auth (MVC web) ─────────────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath         = "/Account/Login";
    options.AccessDeniedPath  = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan    = TimeSpan.FromHours(8);
    options.Events.OnRedirectToLogin = ctx =>
    {
        if (ctx.Request.Path.StartsWithSegments("/api"))
        {
            ctx.Response.StatusCode = 401;
            return Task.CompletedTask;
        }
        ctx.Response.Redirect(ctx.RedirectUri);
        return Task.CompletedTask;
    };
});

builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.Zero;
});

// ── JWT auth (API REST) ───────────────────────────────────
var jwtKey    = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAud    = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtIssuer,
            ValidAudience            = jwtAud,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// ── Swagger / OpenAPI ─────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "UniLife API",
        Version     = "v1",
        Description = "API REST para la plataforma universitaria UniLife"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT en el header Authorization. Ejemplo: **Bearer {token}**",
        Name        = "Authorization",
        In          = ParameterLocation.Header,
        Type        = SecuritySchemeType.ApiKey,
        Scheme      = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── CORS ──────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("ApiPolicy", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UniLife API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("ApiPolicy");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();

    await SeedData.InicializarAsync(services);

    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    foreach (var usuario in userManager.Users.ToList())
        await userManager.UpdateSecurityStampAsync(usuario);
}

app.Run();
