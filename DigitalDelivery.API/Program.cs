using DigitalDelivery.Application;
using DigitalDelivery.Application.Services;
using DigitalDelivery.Application.Settings;
using DigitalDelivery.Infrastructure.EF;
using DigitalDelivery.Infrastructure.Queues;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(option =>
{
    option.AddPolicy("DigitalDeliveryPolicy", builder =>
    {
        builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var secretKey = builder.Configuration.GetValue<string>("AuthSettings:SecretKey");
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateAudience = false,
        ValidateIssuer = false,
        ValidateLifetime = true
    };
});

builder.Configuration.AddUserSecrets<Program>();
builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("AuthSettings"));
builder.Services.Configure<PackageRestrictionSettings>(builder.Configuration.GetSection("PackagerestrictionSettings"));
builder.Services.Configure<MapSettings>(builder.Configuration.GetSection("MapSettings"));
builder.Services.Configure<BaseDeliverySettings>(builder.Configuration.GetSection("BaseDeliverySettings"));

builder.Services.AddSingleton<IOrderQueue, InMemoryOrderQueue>();
builder.Services.AddHostedService<OrderProcessingService>();

builder.Services.AddApplicationServices();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("DigitalDeliveryPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();