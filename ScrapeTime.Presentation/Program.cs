using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.OpenApi.Models;
using ScrapeTime.Presentation.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new AllowAnonymousFilter());
});
builder.Services.AddScoped<IInstagramService, InstagramService>();
builder.Services.AddScoped<IYoutubeService, YoutubeService>();
builder.Services.AddScoped<ITikTokService, TikTokService>();
builder.Services.AddHttpClient();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ScrapeTime API",
        Version = "v1"
    });
});

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Access-Control-Allow-Origin", "Access-Control-Allow-Headers")
            .WithMethods("GET", "POST", "OPTIONS", "PUT", "DELETE"); 
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection(); 

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();