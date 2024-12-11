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
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Services;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Log = Serilog.Log;

var builder = WebApplication.CreateBuilder(args);
//Serilog initialize
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(new JsonFormatter(), "important.json", restrictedToMinimumLevel: LogEventLevel.Warning)
    .WriteTo.File("Logs/all-.logs", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, retainedFileCountLimit: null)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .CreateLogger();

// Add services to the container.
builder.Host.UseSerilog();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
}).ConfigureApiBehaviorOptions(options =>
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
}); ;
builder.Services.AddDbContext<DatabaseContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
}, ServiceLifetime.Singleton);

builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<LoginService>();
builder.Services.AddSingleton<AccountService>();
builder.Services.AddSingleton<ExecutionService>();
builder.Services.AddSingleton<SymbolService>();
builder.Services.AddSingleton<OrderService>();
builder.Services.AddSingleton<PositionService>();
builder.Services.AddSingleton<RiskUserService>();
builder.Services.AddSingleton<ExecutionManager>();
builder.Services.AddSingleton<ApiService>();
builder.Services.AddSingleton<SessionManager>();
builder.Services.AddTransient<IApiKeyValidation, ApiKeyValidation>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRiskUserService, RiskUserService>();
builder.Services.AddScoped<IGatewayService, GatewayService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<ISymbolService, SymbolService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPositionService, PositionService>();
builder.Services.AddScoped<IPasswordHasher<RiskUser>, PasswordHasher<RiskUser>>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

//Add service signalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
}).AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddIdentity<AppUser, IdentityRole>(opt =>
{
    opt.Password.RequireDigit = true;
    opt.Password.RequireLowercase = true;
    opt.Password.RequireUppercase = true;
    opt.Password.RequireNonAlphanumeric = true;
}).AddEntityFrameworkStores<DatabaseContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme =
  opt.DefaultChallengeScheme =
  opt.DefaultForbidScheme =
  opt.DefaultScheme =
  opt.DefaultSignInScheme =
  opt.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = false,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])),
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };


});

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseCors(options =>
      options.AllowAnyOrigin()
      .AllowAnyMethod()
      .AllowAnyHeader());


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

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<TradeHub>("/trade");

app.Run();
