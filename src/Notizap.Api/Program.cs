using System.Text;
using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Notizap.Api.Middlewares;
using Notizap.API.Extensions;
using Notizap.Application.Mapping;
using QuestPDF.Infrastructure;
using Serilog;

// Early initialization de Serilog (ANTES de crear el builder)
// Este logger básico captura errores de configuración
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("🚀 Iniciando aplicación Notizap...");

    var builder = WebApplication.CreateBuilder(args);

    // Configurar Serilog desde appsettings.json
    builder.Host.UseSerilog((context, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins(
                "http://localhost:5173",
                "https://icy-water-08037f110.6.azurestaticapps.net",
                "https://notizap.app",
                "https://www.notizap.app"
                )
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    // Controllers
    builder.Services.AddControllers();

    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    var mapperConfig = new MapperConfiguration(cfg =>
    {
        cfg.AddProfile<MappingProfile>();
    });

    IMapper mapper = mapperConfig.CreateMapper();
    builder.Services.AddSingleton(mapper);

    // Versionado de API
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    });

    // API Explorer para Swagger con versiones
    builder.Services.AddVersionedApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    // Swagger
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Notizap API",
            Version = "v1",
            Description = "Documentación de la API versionada de Notizap"
        });

        options.EnableAnnotations();
            
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header usando el esquema Bearer. \r\n\r\n" +
                          "Escribí: 'Bearer {token}' (sin comillas).",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });
    });

    // Inyección de dependencias
    builder.Services.AddHttpClient();
    builder.Services.AddNotizapServices(builder.Configuration);        

    // DbContext
    builder.Services.AddDbContext<NotizapDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            var key = builder.Configuration["Jwt:Key"];
            var issuer = builder.Configuration["Jwt:Issuer"];
            var audience = builder.Configuration["Jwt:Audience"];

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!))
            };
        });

    builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    });    

    builder.Services.Configure<OcaSettings>(
        builder.Configuration.GetSection("OcaSettings")
    );
    builder.Services.Configure<OcaOperativasSettings>(
        builder.Configuration.GetSection("OcaOperativas"));

    var app = builder.Build();

    // Agregar Serilog request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.GetLevel = (httpContext, elapsed, ex) => ex != null 
            ? Serilog.Events.LogEventLevel.Error 
            : Serilog.Events.LogEventLevel.Information;
    });
    app.UseMiddleware<RequestSeparatorMiddleware>();

    // Obtener proveedor de versiones de API
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    QuestPDF.Settings.License = LicenseType.Community;
    QuestPDF.Settings.EnableDebugging = true;

    // Middleware
    app.UseCors("AllowFrontend");
    app.UseHttpsRedirection();

    app.UseMiddleware<ExceptionMiddleware>();

    // Swagger con versiones
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"Notizap API {description.GroupName.ToUpperInvariant()}"
            );
        }
    });

    app.MapControllers();
    app.UseAuthentication();
    app.UseAuthorization();

    Log.Information("✅ Notizap API iniciada correctamente");
    app.Run();
}
catch (Exception ex)
{
    // Capturar errores fatales de inicio
    Log.Fatal(ex, "💥 Error fatal al iniciar la aplicación");
}
finally
{
    // Limpiar recursos de Serilog al cerrar
    await Log.CloseAndFlushAsync();
}