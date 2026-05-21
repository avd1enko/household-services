using NotificationService.Application.DTOs;
using NotificationService.Application.Services;
using NotificationService.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddScoped<ISmsNotificationService, SmsNotificationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json/", "NotificationService API");
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();