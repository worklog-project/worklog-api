using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Negotiate;
using worklog_api.Controllers;
using worklog_api.Service;
using worklog_api.Repository;
using Microsoft.EntityFrameworkCore;
using worklog_api.error;
using worklog_api.filters;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<IMOLRepository, MOLRepository>();
builder.Services.AddScoped<ILWORepository, LWORepository>();
builder.Services.AddScoped<IMOLTrackingHistoryRepository, MOLTrackingHistoryRepository>();

builder.Services.AddScoped<IMOLService, MOLService>();
builder.Services.AddScoped<ILWOService, LWOService>();
builder.Services.AddScoped<IUserService,UserService>();
builder.Services.AddScoped<JWTConfiguration>();
builder.Services.AddScoped<IMOLTrackingHistoryService, MOLTrackingHistoryService>();
builder.Services.AddSingleton<string>(provider => "Server=worklog-staging.database.windows.net,1433;Initial Catalog=worklog;Persist Security Info=False;User ID=worklog;Password=Superadmin123@;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");



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

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers(); // This maps your controllers automatically.
Console.WriteLine("APP RUN SUCCESSFULLY");
app.Run();
