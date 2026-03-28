using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using LaundryManagement.API.Middleware;
using LaundryManagement.Application;
using LaundryManagement.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

// Configurar Serilog antes de crear el builder
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        "logs/app-{yyyy-MM-dd}.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u5}] {Message:lj}{NewLine}{Exception}",
        retainedFileCountLimit: 30
    )
    .WriteTo.File(
        "logs/errors-{yyyy-MM-dd}.txt",
        levelSwitch: new Serilog.Core.LoggingLevelSwitch(Serilog.Events.LogEventLevel.Error),
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u5}] {Message:lj}{NewLine}{Exception}",
        retainedFileCountLimit: 30
    )
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Reemplazar el logging estándar con Serilog
builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Add Application layer services (MediatR, FluentValidation, AutoMapper)
builder.Services.AddApplication();

// Add Infrastructure layer services (DbContext, Dapper)
builder.Services.AddInfrastructure(builder.Configuration);

// Add JWT Authentication
var secretKey = builder.Configuration["JwtSettings:SecretKey"]
    ?? throw new InvalidOperationException("JwtSettings:SecretKey not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // No tolerance for expiration
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("Authentication failed: {Message}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                var username = context.Principal?.Identity?.Name ?? "Unknown";
                logger.LogInformation("Token validated for user: {Username}", username);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Laundry Management API",
        Version = "v1",
        Description = "API REST para sistema de gestión de lavandería con arquitectura DDD y CQRS.\n\n" +
                      "Esta API utiliza stored procedures para operaciones críticas:\n" +
                      "- **Órdenes**: Gestión completa de órdenes de lavandería\n" +
                      "- **Pagos**: Registro y consulta de pagos\n" +
                      "- **Cortes de Caja**: Control de cierres de caja\n" +
                      "- **Reportes**: Análisis de ventas y operaciones\n" +
                      "- **Ubicaciones**: Gestión de almacén",
        Contact = new Microsoft.OpenApi.OpenApiContact
        {
            Name = "Equipo de Desarrollo",
            Email = "desarrollo@lavanderia.com"
        },
        License = new Microsoft.OpenApi.OpenApiLicense
        {
            Name = "Uso privado"
        }
    });

    // Include XML comments from API project
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Include XML comments from Application project
    var applicationXmlFile = "LaundryManagement.Application.xml";
    var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, applicationXmlFile);
    if (File.Exists(applicationXmlPath))
    {
        options.IncludeXmlComments(applicationXmlPath);
    }

    // Group endpoints by controller
    options.TagActionsBy(api =>
    {
        if (api.GroupName != null)
            return new[] { api.GroupName };

        var controllerName = api.ActionDescriptor.RouteValues["controller"];
        return new[] { controllerName ?? "Default" };
    });

    options.DocInclusionPredicate((name, api) => true);

    // Enable annotations
    options.EnableAnnotations();
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// QuestPDF — Community license
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var app = builder.Build();

// Global exception handler middleware
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Laundry Management API v1");
        options.RoutePrefix = string.Empty; // Swagger UI en la raíz
        options.DocumentTitle = "Laundry Management API - Documentación";
        options.DefaultModelsExpandDepth(2);
        options.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Model);
        options.DisplayRequestDuration();
        options.EnableDeepLinking();
        options.EnableFilter();
        options.ShowExtensions();
    });
}

// IMPORTANTE: UseCors DEBE estar antes de UseHttpsRedirection y UseAuthentication/UseAuthorization
app.UseCors("AllowReactApp");

// Solo redireccionar a HTTPS en producción
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
