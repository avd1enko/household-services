var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
var app = builder.Build();


// ваггер будет существовать только при статусе проекта IsDevelopment. при смене статуса сваггера уже не будет
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