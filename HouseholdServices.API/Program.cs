using System.Text;
using HouseholdServices.Application.Services.Auth;
using HouseholdServices.Application.Services.MasterProfiles;
using HouseholdServices.Application.Services.Notification;
using HouseholdServices.Application.Services.Order;
using HouseholdServices.Application.Services.Request;
using HouseholdServices.Application.Services.Responses;
using HouseholdServices.Application.Services.Reviews;
using HouseholdServices.Application.Services.UserProfiles;
using HouseholdServices.Application.Services.Users;
using HouseholdServices.Domain.Entities;
using HouseholdServices.Infrastructure.Data;
using HouseholdServices.Infrastructure.Services.Auth;
using HouseholdServices.Infrastructure.Services.MasterProfiles;
using HouseholdServices.Infrastructure.Services.Notification;
using HouseholdServices.Infrastructure.Services.Orders;
using HouseholdServices.Infrastructure.Services.Request;
using HouseholdServices.Infrastructure.Services.Responses;
using HouseholdServices.Infrastructure.Services.Reviews;
using HouseholdServices.Infrastructure.Services.UserProfiles;
using HouseholdServices.Infrastructure.Services.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Household Services API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token like this: Bearer {your token}"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });


});

builder.Services.AddScoped<IAuthService, AuthService>(); // когда где-то просят IAuthService - создай AuthService и передай его туда
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IResponseService, ResponseService>();
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IMasterProfileService, MasterProfileService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173",
                "https://household-services-frontend.onrender.com")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();


// сваггер будет существовать только при статусе проекта IsDevelopment. при смене статуса сваггера уже не будет
// статус задается через перременную среды в файле api проекта launchSettings.json в поле "ASPNETCORE_ENVIRONMENT": "Development"
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Household Services API");
    });


}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

