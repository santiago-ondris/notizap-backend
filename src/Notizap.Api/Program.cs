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
using Notizap.Application.Mapping;
using Notizap.Infrastructure.Services;
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
builder.Services.AddScoped<IMailchimpService, MailchimpService>();
builder.Services.AddScoped<IWooCommerceService, WooCommerceService>();
builder.Services.AddScoped<IMercadoLibreService, MercadoLibreService>();
builder.Services.AddScoped<IReelsService, ReelsService>();
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGastoService, GastoService>();
builder.Services.Configure<MetricoolSettings>(
    builder.Configuration.GetSection("Metricool"));
builder.Services.AddScoped<IReelsService, ReelsService>();
builder.Services.AddScoped<IFollowersService, FollowersService>();
builder.Services.AddScoped<IStoriesService, StoriesService>();
builder.Services.AddScoped<IPostsService, PostsService>();

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

var app = builder.Build();

// Obtener proveedor de versiones de API
var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

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
app.Run();
