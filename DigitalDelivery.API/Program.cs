using DigitalDelivery.Application;
using DigitalDelivery.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;

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

builder.Services.AddApplicationServices();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("DigitalDeliveryPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();