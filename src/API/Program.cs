using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using API.Database;
using API.Database.Entities;
using API.Services.Auth;
using API.Services.Seed;
using Scalar.AspNetCore;
using System.Text;

// =================================== Configuración ===================================

var builder = WebApplication.CreateBuilder(args);

// Telemetría y conexión a la base de datos
builder.AddServiceDefaults();                     
builder.AddNpgsqlDbContext<AppDbContext>("appdb");

// Servicios de ASP.NET Core
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Servicios personalizados
builder.Services.AddTransient<IJwtService, JwtService>();
builder.Services.AddTransient<ISeedService, SeedService>();

// Configuración de IdentityUser
builder.Services.AddIdentity<UserEntity, IdentityRole>(options =>
{ options.SignIn.RequireConfirmedAccount = false; }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

builder.Services.AddAuthentication(options => 
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ClockSkew = TimeSpan.Zero,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"] ?? ""))
    };
});

// Configuración de CORS
builder.Services.AddCors(options =>
{
    var allowURLS = builder.Configuration.GetSection("AllowUrls").Get<string[]>();

    options.AddPolicy("CorsPolicy", builder => builder
    .WithOrigins(allowURLS ?? [])
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());
});

// ===================================== Ejecución =====================================

var app = builder.Build();

app.MapDefaultEndpoints();

// Migración automatica
using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();

// Cargar roles de usuario (RolesConstant.cs)
using (var scope = app.Services.CreateScope())
    await scope.ServiceProvider.GetRequiredService<ISeedService>().LoadRolesAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
