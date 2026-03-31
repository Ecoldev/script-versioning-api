using script_versioning_api.Services;
using script_versioning_api.Auth;

var builder = WebApplication.CreateBuilder(args);

// GitLab
builder.Services.AddHttpClient<GitLabService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// wybór auth (PRZED BUILD)
var authType = builder.Configuration["GitLab:AuthType"];

if (authType == "Env")
{
    builder.Services.AddSingleton<IGitLabAuthProvider, EnvAuthProvider>();
}
else if (authType == "SecretApi")
{
    builder.Services.AddHttpClient<SecretApiAuthProvider>();
    builder.Services.AddSingleton<IGitLabAuthProvider, SecretApiAuthProvider>();
}

// 🔥 build dopiero teraz
var app = builder.Build();

// middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/weatherforecast", () =>
{
    var summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild",
        "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        )).ToArray();

    return forecast;
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}