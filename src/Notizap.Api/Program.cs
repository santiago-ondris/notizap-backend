using Microsoft.EntityFrameworkCore;
using NotiZap.Dashboard.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.Configure<MailchimpSettings>(
    builder.Configuration.GetSection("MailchimpSettings"));
builder.Services.AddHttpClient<IMailchimpService, MailchimpService>();

builder.Services.Configure<WooCommerceSettings>(
    builder.Configuration.GetSection("WooCommerce"));
builder.Services.AddHttpClient(); 
builder.Services.AddScoped<IWooCommerceService, WooCommerceService>();

builder.Services.AddScoped<IMercadoLibreService, MercadoLibreService>();

builder.Services.AddDbContext<NotizapDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IReelsService, ReelsService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();

var app = builder.Build();


app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
