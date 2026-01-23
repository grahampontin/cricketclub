#nullable disable
using CricketClubDAL;
using log4net;
using log4net.Config;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Configure log4net
var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

// Bridge connection strings from appsettings.json to environment variables for legacy DAL
var connectionStrings = builder.Configuration.GetSection("ConnectionStrings");
foreach (var connectionString in connectionStrings.GetChildren())
{
    var envVarName = $"ConnectionStrings__{connectionString.Key}";
    var connStr = connectionString.Value;
    
    // Remove Provider parameter if present (not supported by SqlClient, only by OleDb)
    if (!string.IsNullOrEmpty(connStr) && connStr.Contains("Provider=", StringComparison.OrdinalIgnoreCase))
    {
        connStr = System.Text.RegularExpressions.Regex.Replace(
            connStr, 
            @"Provider\s*=\s*[^;]+;?", 
            "", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
    
    // Add TrustServerCertificate=True if not already present (for development environments with self-signed certs)
    if (!string.IsNullOrEmpty(connStr) && !connStr.Contains("TrustServerCertificate", StringComparison.OrdinalIgnoreCase))
    {
        connStr = connStr.TrimEnd(';') + ";TrustServerCertificate=True";
    }
    
    Environment.SetEnvironmentVariable(envVarName, connStr);
}

// Add services to the container
builder.Services.AddControllers();

// Register IDao for dependency injection
builder.Services.AddScoped<IDao, Dao>();

// Add Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "The Village CC API",
        Version = "v1",
        Description = "Cricket club management API"
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "The Village CC API v1");
    });
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Make Program class accessible to integration tests
public partial class Program { }
