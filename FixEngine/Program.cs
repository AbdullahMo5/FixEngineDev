using FixEngine.Auth;
using FixEngine.Data;
using FixEngine.Entity;
using FixEngine.Hubs;
using FixEngine.Middlewares;
using FixEngine.Services;
using FixEngine.Shared;
using Managers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
builder.Services.AddSingleton<SymbolService>();
builder.Services.AddSingleton<ExecutionManager>();

builder.Services.AddDbContext<DatabaseContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
}, ServiceLifetime.Singleton);
builder.Services.AddSingleton<ApiService>();
builder.Services.AddSingleton<SessionManager>();
builder.Services.AddTransient<IApiKeyValidation, ApiKeyValidation>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRiskUserService, RiskUserService>();
builder.Services.AddScoped<IGatewayService, GatewayService>();
builder.Services.AddScoped<IGroupService, GroupService>();

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

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])),
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddIdentity<AppUser, IdentityRole>(opt =>
{
    opt.Password.RequireDigit = true;
    opt.Password.RequireLowercase = true;
    opt.Password.RequireUppercase = true;
    opt.Password.RequireNonAlphanumeric = true;
}).AddEntityFrameworkStores<DatabaseContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState.Values
                        .SelectMany(x => x.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToArray();

                    return new BadRequestObjectResult(new
                    {
                        Message = "Validation failed",
                        Errors = errors
                    });
                };
            });

builder.Services.AddAuthorization();
var app = builder.Build();
//ApiCredentials apiCredentials = new ApiCredentials(
//    QuoteHost: "crfuk.centroidsol.com",
//    TradeHost: "crfuk.centroidsol.com",
//    QuotePort: 53810,
//    TradePort: 53811,
//    QuoteSenderCompId: "MD_Fintic-FIX-TEST",
//    TradeSenderCompId: "TD_Fintic-FIX-TEST",
//    null,
//    null,
//    //QuoteSenderSubId: "testcentroid",// + token,
//    //TradeSenderSubId: "testcentroid",// + token,
//    QuoteTargetCompId: "CENTROID_SOL",
//    TradeTargetCompId: "CENTROID_SOL",
//    null,
//    null,
//    //QuoteTargetSubId: "QUOTE",
//    //TradeTargetSubId: "TRADE",
//    QuoteUsername: "Fintic-FIX-TEST",
//    QuotePassword: "#oB*sFb6",
//    TradeUsername: "Fintic-FIX-TEST",
//    TradePassword: "#oB*sFb6", //"123Nm,.com",
//    TradeResetOnLogin: "N",
//    TradeSsl: "Y",
//    QuoteResetOnLogin: "Y",
//    QuoteSsl: "N",
//    Account: "Fintic-Fix-Test"
//    );
//var apiService = app.Services.GetRequiredService<ApiService>();
//await apiService.ConnectClient(apiCredentials, Guid.NewGuid().ToString(), "CENTROID");

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
    options.WithOrigins("http://localhost:3000", "http://127.0.0.1:5000")
    .AllowAnyHeader()
    .WithMethods("GET", "POST")
    .AllowCredentials());

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.MapHub<TradeHub>("/trade");

app.Run();
