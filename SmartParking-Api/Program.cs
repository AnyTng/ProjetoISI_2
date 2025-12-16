using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SoapCore;
using SmartParking_Api.Auth;
using SmartParking_Api.Data;
using SmartParking_Api.Models;
using SmartParking_Api.Services;
using SmartParking_Api.Services.Soap;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// DB
// --------------------
var connectionString = builder.Configuration.GetConnectionString("AzureBDConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("ConnectionString 'AzureBDConnection' em falta.");

builder.Services.AddDbContext<SmartParkingDbContext>(options =>
    options.UseSqlServer(connectionString));

// --------------------
// External Services
// --------------------
builder.Services.AddHttpClient<IWeatherService, OpenMeteoWeatherService>();

// --------------------
// SOAP
// --------------------
builder.Services.AddSoapCore();
builder.Services.AddScoped<ISmartParkingSoapService, SmartParkingSoapService>();

// --------------------
// Auth (JWT + ApiKey) + Password hashing (tabela Users tua)
// --------------------
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("Config 'Jwt:Key' em falta.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],

        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],

        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30)
    };
})
.AddScheme<AuthenticationSchemeOptions, ApiKeyAuthHandler>(ApiKeyDefaults.SchemeName, _ => { });

builder.Services.AddAuthorization();

// --------------------
// REST + Swagger
// --------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartParking-Api", Version = "v1" });

    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        Name = ApiKeyDefaults.HeaderName, // "X-Api-Key"
        In = ParameterLocation.Header,
        Description = "Sensor API key"
    });

    // Swashbuckle v10: requirement via delegate + OpenApiSecuritySchemeReference :contentReference[oaicite:1]{index=1}
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = new List<string>()
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("ApiKey", document)] = new List<string>()
    });
});

var app = builder.Build();

// --------------------
// Pipeline
// --------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.EnablePersistAuthorization());
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// SOAP
((IEndpointRouteBuilder)app).UseSoapEndpoint<ISmartParkingSoapService>(
    "/soap/smartparking.asmx",
    new SoapEncoderOptions(),
    SoapSerializer.XmlSerializer);

// REST
app.MapControllers();

app.Run();
