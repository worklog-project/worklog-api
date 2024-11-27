using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Negotiate;
using worklog_api.Controllers;
using worklog_api.Service;
using worklog_api.Repository;
using Microsoft.EntityFrameworkCore;
using worklog_api.error;
using worklog_api.filters;
using worklog_api.Repository.implementation;
using worklog_api.Service.implementation;
using worklog_api.helper;
using DotNetEnv; // Add this for .env support

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
Env.Load();  // This will automatically load the .env file from the root of the project

// Add logging service
builder.Services.AddLogging();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<IMOLRepository, MOLRepository>();
builder.Services.AddScoped<ILWORepository, LWORepository>();
builder.Services.AddScoped<IMOLTrackingHistoryRepository, MOLTrackingHistoryRepository>();
builder.Services.AddScoped<IStatusHistoryRepository, StatusHistoryRepository>();
builder.Services.AddScoped<IDailyRepository, DailyRepository>();
builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();
builder.Services.AddScoped<IBacklogRepository, BacklogRepository>();

builder.Services.AddScoped<IMOLService, MOLService>();
builder.Services.AddScoped<ILWOService, LWOService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<JWTConfiguration>();
builder.Services.AddScoped<IMOLTrackingHistoryService, MOLTrackingHistoryService>();
builder.Services.AddScoped<IDailyService, DailyService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IBacklogService, BacklogService>();
builder.Services.AddScoped<IFileUploadHelper, FileUploadHelper>();

// Use environment variable for connection string
string connectionString = Env.GetString("DB_CONNECTION_STRING"); // Read from .env file
builder.Services.AddSingleton<string>(provider => connectionString);
builder.Services.AddSingleton(new JwtSecurityTokenHandler());
builder.Services.AddSingleton(new DateHelper());

// Log the connection string (Note: Be careful with logging sensitive information in production)
var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
logger.LogInformation("Database connection string: {ConnectionString}", connectionString);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Call the JWT configuration method
var serviceProvider = builder.Services.BuildServiceProvider();
// Resolve JWTConfiguration and call AddJWTConfiguration
var jwtConfig = serviceProvider.GetRequiredService<JWTConfiguration>();
jwtConfig.ConfigureServices(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Add your custom exception middleware
app.ConfigureCustomExceptionMiddleware();
app.UseCors("AllowAllOrigins");
// app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

app.MapControllers(); // This maps your controllers automatically.
Console.WriteLine("APP RUN SUCCESSFULLY");
app.Run();
