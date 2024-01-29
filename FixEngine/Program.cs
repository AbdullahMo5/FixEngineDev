using FixEngine.Auth;
using FixEngine.Data;
using FixEngine.Hubs;
using FixEngine.Middlewares;
using FixEngine.Services;
using FixEngine.Shared;
using Managers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Services;
using System.Text;
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
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<LoginService>();
builder.Services.AddSingleton<AccountService>();
builder.Services.AddSingleton<ExecutionService>();
builder.Services.AddSingleton<ExecutionManager>();

builder.Services.AddDbContext<DatabaseContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
},ServiceLifetime.Singleton);
builder.Services.AddSingleton<ApiService>();
builder.Services.AddSingleton<SessionManager>();
builder.Services.AddTransient<IApiKeyValidation, ApiKeyValidation>();

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

builder.Services.AddAuthentication(options => { 
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters{
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer=true,
        ValidateAudience=true,
        ValidateLifetime=true,
        ValidateIssuerSigningKey=true
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseWhen(context => context.Request.Path.StartsWithSegments("/api/secure"), appBuilder =>
{
    appBuilder.UseMiddleware<ApiKeyValidationMiddleware>();
});

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<TradeHub>("/trade");

app.Run();
