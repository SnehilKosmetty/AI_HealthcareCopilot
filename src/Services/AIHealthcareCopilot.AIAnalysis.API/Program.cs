using Azure;
using Azure.AI.TextAnalytics;
using AIHealthcareCopilot.Shared.Contracts;
using AIHealthcareCopilot.AIAnalysis.API.Services;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AI Analysis API", Version = "v1" });
    // Allow testing via direct service or through the Gateway from the same Swagger UI
    c.AddServer(new Microsoft.OpenApi.Models.OpenApiServer { Url = "https://localhost:7001" }); // direct
    c.AddServer(new Microsoft.OpenApi.Models.OpenApiServer { Url = "https://localhost:7000" }); // gateway

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// Azure Text Analytics configuration and client factory
builder.Services.AddSingleton(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var endpoint = config["AzureTextAnalytics:Endpoint"];
    var key = config["AzureTextAnalytics:Key"];
    if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(key))
    {
        return null; // Allows service to run in fallback mode
    }
    return new TextAnalyticsClient(new Uri(endpoint), new AzureKeyCredential(key));
});

builder.Services.AddScoped<IAnalysisService, AzureTextAnalyticsAnalysisService>();

// HttpClient to call PatientRecords via Gateway
builder.Services.AddHttpClient("PatientsApi", (provider, client) =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var baseUrl = config["GatewayBaseUrl"] ?? "https://localhost:7000";
    client.BaseAddress = new Uri(baseUrl);
});

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var cfg = builder.Configuration;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = cfg["Jwt:Issuer"],
            ValidAudience = cfg["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:Key"]!))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.MapControllers();
app.Run();
