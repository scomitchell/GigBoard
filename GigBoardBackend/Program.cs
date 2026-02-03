using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using dotenv.net;
using System.Text;
using Hangfire;
using GigBoardBackend.Data;
using GigBoardBackend.Services;
using System.Text.Json.Serialization;
using GigBoard.Hubs;

var builder = WebApplication.CreateBuilder(args);
DotEnv.Load();

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Add CORS
var allowedOrigin = Environment.GetEnvironmentVariable("ALLOWED_ORIGIN") ?? "http://localhost:5173";

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigin)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Add Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Authentication
// Get secret key and encode
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? throw new InvalidOperationException("JWT_SECRET_KEY environment variable is not set.");
var key = Encoding.UTF8.GetBytes(secretKey);

// Add Authentication to services
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/statistics"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Add custom services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<StatisticsService>();

// Add HTTP Client
builder.Services.AddHttpClient();

// Hangfire
var hangfireConnection = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddHangfire(config =>
    config.UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(hangfireConnection));

builder.Services.AddHangfireServer();
builder.Services.AddTransient<ShiftTrainingJob>();

// Add controllers
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SignalR
builder.Services.AddSignalR();

// Build App
var app = builder.Build();

// Use Cors policy
app.UseCors("AllowFrontend");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate(); // Automatically runs all migrations
}

app.UseAuthentication();
app.UseAuthorization();

// Enable Hangfire dashboard
app.UseHangfireDashboard("/hangfire");


// Set hub to /hub/statistics
app.MapHub<StatisticsHub>("/hubs/statistics")
    .RequireAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    var job = scope.ServiceProvider.GetRequiredService<ShiftTrainingJob>();

    recurringJobManager.AddOrUpdate(
        "train-shift-model",
        () => job.TrainShiftModelJob(),
        Cron.Daily
    );
}

app.Run();
