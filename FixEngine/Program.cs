using FixEngine.Data;
using FixEngine.Hubs;
using FixEngine.Services;
using Managers;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Services;
using System.Text.Json;
using Log = Serilog.Log;

var builder = WebApplication.CreateBuilder(args);
//cors policy setup
//https://learn.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-8.0
//Serilog initialize
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(new JsonFormatter(), "important.json", restrictedToMinimumLevel: LogEventLevel.Warning)
    .WriteTo.File("Logs/all-.logs", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, retainedFileCountLimit: null)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .CreateLogger();

// Add services to the container.
builder.Host.UseSerilog();
builder.Services.AddControllers();
builder.Services.AddSingleton<ExecutionService>();
builder.Services.AddSingleton<ExecutionManager>();

builder.Services.AddDbContext<DatabaseContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
},ServiceLifetime.Singleton);
builder.Services.AddSingleton<ApiService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Add service signalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
}).AddJsonProtocol(options =>
{    
    options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});


var app = builder.Build();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(options =>
    options.WithOrigins("http://localhost:3000")
    .AllowAnyHeader()
    .WithMethods("GET", "POST")
    .AllowCredentials());
            
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<TradeHub>("/trade");

app.Run();
