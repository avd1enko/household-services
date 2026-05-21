using HouseholdServices.Application.Services.Auth;
using Microsoft.EntityFrameworkCore;
using HouseholdServices.Infrastructure.Data;
using HouseholdServices.Infrastructure.Services.Auth;
using HouseholdServices.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Text;
using HouseholdServices.Application.Services.Notification;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using HouseholdServices.Application.Services.Users;
using HouseholdServices.Infrastructure.Services.Users;
using HouseholdServices.Infrastructure.Services.Notification;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddScoped<IAuthService, AuthService>(); // когда где-то просят IAuthService - создай AuthService и передай его туда
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// временно (для теста)
builder.Services.AddScoped<INotificationTestService, NotificationTestService>();

// настройка JWT

string jwtKey = builder.Configuration["Jwt:Key"]!;
string jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
string jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

// наш контекст работы с бд, настраиваемый в HouseholdServices.Infrastructure.data. ПОдключение к postgresql
// будем использовать через dependency injection
builder.Services.AddDbContext<HouseholdServicesDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// так как мы ожидаем созданный httpclient внутри нашего сервиса, то нам нужно создать и выдать настроенный httpclient,
// чтобы было что передавать через dependency injection
builder.Services.AddHttpClient<INotificationClient, NotificationClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["NotificationService:BaseUrl"]!);
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

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();