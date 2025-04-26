using Notizap.Services.MercadoLibre;
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
builder.Services.AddHttpClient<IWooCommerceService, WooCommerceService>();

builder.Services.Configure<MercadoLibreSettings>(
    builder.Configuration.GetSection("MercadoLibreSettings"));
builder.Services.AddHttpClient<IMercadoLibreService, MercadoLibreService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();


app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
