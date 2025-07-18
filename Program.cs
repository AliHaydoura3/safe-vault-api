using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add EF Core with in-memory DB
builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("AuthDb"));

// Add Identity
builder
    .Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Add JWT Authentication
var key = Encoding.UTF8.GetBytes("super_secret_key_123!");
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Seed roles and a test admin user
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    foreach (var role in new[] { "Admin", "User" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    var admin = new IdentityUser { UserName = "Ali", Email = "ali@example.com" };
    var result = await userManager.CreateAsync(admin, "SecurePassword123!");
    if (result.Succeeded)
        await userManager.AddToRoleAsync(admin, "Admin");
}

app.UseAuthentication();
app.UseAuthorization();

app.MapPost(
    "/register",
    async (
        UserManager<IdentityUser> userManager,
        string username,
        string email,
        string password,
        string role
    ) =>
    {
        var user = new IdentityUser { UserName = username, Email = email };
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);

        await userManager.AddToRoleAsync(user, role);
        return Results.Ok("User registered with role.");
    }
);

app.MapPost(
    "/login",
    async (UserManager<IdentityUser> userManager, string username, string password) =>
    {
        var user = await userManager.FindByNameAsync(username);
        if (user == null || !await userManager.CheckPasswordAsync(user, password))
            return Results.Unauthorized();

        var roles = await userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256
            )
        );

        var tokenString = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(
            token
        );
        return Results.Ok(new { token = tokenString });
    }
);

app.MapGet(
    "/admin-only",
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
() =>
    {
        return Results.Ok("Welcome, Admin!");
    }
);

app.MapGet(
    "/user-only",
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "User")]
() =>
    {
        return Results.Ok("Welcome, User!");
    }
);

app.Run();

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }
}
