using Microsoft.AspNetCore.Authentication.Negotiate;
using worklog_api.Controllers;
using worklog_api.Service;
using worklog_api.Repository;
using Microsoft.EntityFrameworkCore;
using worklog_api.error;
using worklog_api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);


// Register DbContext with the connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<IMOLRepository, MOLRepository>();
builder.Services.AddScoped<IMOLService, MOLService>();
builder.Services.AddSingleton<string>(provider => "Server=localhost,1433;Database=worklog;User Id=sa;Password=Superadmin123@;Encrypt=False;");



// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
//   .AddNegotiate();

//builder.Services.AddAuthorization(options =>
//{
//    // By default, all incoming requests will be authorized according to the default policy.
//    options.FallbackPolicy = options.DefaultPolicy;
//});

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
app.UseAuthorization();
app.MapControllers(); // This maps your controllers automatically.
app.Run();
