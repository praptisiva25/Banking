
using System.Text;
using BankingAPI.Data;
using BankingAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.User.RequireUniqueEmail = true;

        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<IdentityRole<int>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT key is missing.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),

            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddDataProtection();
builder.Services.AddOpenApi();

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/db-test", async (ApplicationDbContext db) =>
{
    var connected = await db.Database.CanConnectAsync();

    return connected
        ? Results.Ok("Connected to Neon PostgreSQL!")
        : Results.Problem("Could not connect to Neon.");
});

app.MapControllers();

app.MapGet("/api/auth/me", [Microsoft.AspNetCore.Authorization.Authorize] 
    (System.Security.Claims.ClaimsPrincipal user) =>
{
    return Results.Ok(new
    {
        id = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
        name = user.Identity?.Name,
        email = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
    });
});

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager = services
        .GetRequiredService<RoleManager<IdentityRole<int>>>();

    var userManager = services
        .GetRequiredService<UserManager<ApplicationUser>>();

    var db = services
        .GetRequiredService<ApplicationDbContext>();

    await RoleSeeder.SeedAsync(roleManager, userManager);
    await AdminSeeder.SeedAsync(userManager, app.Configuration);
    await DbSeeder.SeedAsync(db);
}

app.Run();
