using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using voltajemoda_backend.Services; // O el namespace que aparezca al inicio de tus archivos en Services
using voltajemoda_backend.Models; // O el namespace que aparezca al inicio de tus archivos en Models
using Microsoft.AspNetCore.Mvc;
using voltajemoda_backend.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// 1. Agregar soporte para Controladores
builder.Services.AddControllers();

// Configurar políticas de CORS para integración con Blazor
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorCorsPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5000",
                "https://localhost:7000",
                "http://localhost:5200",
                "https://localhost:7200"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });

    options.AddPolicy("AllowAllDev", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Falta configurar ConnectionStrings:DefaultConnection.");
builder.Services.AddDbContext<VoltajeDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null)));

builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();

builder.Services.AddScoped<IJwtService, JwtService>();

// 2. Configurar OpenAPI (Requisito interno para que Scalar funcione)
builder.Services.AddOpenApi();

// 3. Configurar Autenticación JWT leyendo los datos de secrets.json
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? builder.Configuration["JwtSettings:SecretKey"]
    ?? throw new InvalidOperationException(
        "Falta configurar la clave JWT. Defina Jwt:Key o JwtSettings:SecretKey.");
builder.Configuration["Jwt:Key"] = jwtKey;
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();

// Interceptar las validaciones fallidas de los DTOs
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errores = context.ModelState
            .Where(e => e.Value!.Errors.Count > 0)
            .Select(e => e.Value!.Errors.First().ErrorMessage)
            .ToList();

        return new BadRequestObjectResult(new
        {
            error = true,
            mensaje = "Los datos enviados no son válidos.",
            detalles = errores
        });
    };
});

var app = builder.Build();

// 4. Configurar el Pipeline HTTP (Middlewares)
if (app.Environment.IsDevelopment())
{
    // Genera el documento JSON con la definición de tu API
    app.MapOpenApi();
    // Levanta la interfaz gráfica interactiva en la ruta /scalar
    app.MapScalarApiReference();
    app.UseCors("AllowAllDev");
}
else
{
    app.UseHttpsRedirection();
    app.UseCors("BlazorCorsPolicy");
}

app.UseMiddleware<ExceptionMiddleware>();

// El orden es obligatorio: CORS -> Autenticación -> Autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();