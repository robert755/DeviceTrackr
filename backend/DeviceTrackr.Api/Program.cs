using DeviceTrackr.Api.Configuration;
using DeviceTrackr.Api.Data;
using DeviceTrackr.Api.Repositories;
using DeviceTrackr.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddDbContext<DeviceTrackrDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<DeviceRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<DeviceService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();

builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection(GeminiOptions.SectionName));
builder.Services.AddHttpClient<GeminiDescriptionService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(1);
});

var app = builder.Build();

if (string.IsNullOrWhiteSpace(GeminiConfigHelper.ResolveApiKey(app.Configuration, app.Environment.ContentRootPath)))
{
    app.Logger.LogWarning(
        "Gemini API key is missing after resolving env, merged config, and appsettings files. Set GEMINI_API_KEY / Gemini__ApiKey, or Gemini:ApiKey in appsettings.json (avoid an empty Gemini__ApiKey env var — it overrides the file).");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");

app.MapControllers();

app.Run();
