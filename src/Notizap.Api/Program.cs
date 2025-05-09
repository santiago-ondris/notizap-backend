using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NotiZap.Dashboard.API.Services;

var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Controllers
builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

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
});

// Inyección de dependencias
builder.Services.AddHttpClient();
builder.Services.AddScoped<IMailchimpService, MailchimpService>();
builder.Services.AddScoped<IWooCommerceService, WooCommerceService>();
builder.Services.AddScoped<IMercadoLibreService, MercadoLibreService>();
builder.Services.AddScoped<IReelsService, ReelsService>();
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();

// DbContext
builder.Services.AddDbContext<NotizapDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Obtener proveedor de versiones de API
var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

// Middleware
app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthorization();

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
app.Run();
