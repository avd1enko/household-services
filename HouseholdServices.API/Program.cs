using HouseholdServices.Application.Services.Auth;
using Microsoft.EntityFrameworkCore;
using HouseholdServices.Infrastructure.Data;
using HouseholdServices.Infrastructure.Services.Auth;
using HouseholdServices.Domain.Entities;
using Microsoft.AspNetCore.Identity;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddScoped<IAuthService, AuthService>(); // когда где-то просят IAuthService - создай AuthService и передай его туда
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// наш контекст работы с бд, настраиваемый в HouseholdServices.Infrastructure.data. ПОдключение к postgresql
// будем использовать через dependency injection
builder.Services.AddDbContext<HouseholdServicesDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();


// сваггер будет существовать только при статусе проекта IsDevelopment. при смене статуса сваггера уже не будет
// статус задается через перременную среды в файле api проекта launchSettings.json в поле "ASPNETCORE_ENVIRONMENT": "Development"
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Household Services API");
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();