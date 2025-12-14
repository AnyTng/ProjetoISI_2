using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using SoapCore;
using SmartParking_Api.Auth;
using SmartParking_Api.Data;
using SmartParking_Api.Services;
using SmartParking_Api.Services.Soap;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("AzureBDConnection");

// DbContext (Azure SQL)
builder.Services.AddDbContext<SmartParkingDbContext>(options =>
    options.UseSqlServer(connectionString));

// Weather
builder.Services.AddHttpClient<IWeatherService, OpenMeteoWeatherService>();

// SOAP
builder.Services.AddSoapCore();
builder.Services.AddScoped<ISmartParkingSoapService, SmartParkingSoapService>();

// REST + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartParking-Api", Version = "v1" });

    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key no header. Ex: X-Api-Key: <a_tua_key>",
        Type = SecuritySchemeType.ApiKey,
        Name = "X-Api-Key",
        In = ParameterLocation.Header
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("ApiKey", document)] = new List<string>()
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.EnablePersistAuthorization());
}

app.UseHttpsRedirection();

// API key para tudo exceto swagger (senão o UI não carrega o swagger.json)
app.UseWhen(ctx => !ctx.Request.Path.StartsWithSegments("/swagger"), branch =>
{
    branch.UseMiddleware<ApiKeyCheck>();
});

// SOAP
var routes = (IEndpointRouteBuilder)app;
routes.UseSoapEndpoint<ISmartParkingSoapService>(
    "/soap/smartparking.asmx",
    new SoapEncoderOptions(),
    SoapSerializer.XmlSerializer);

// REST
app.MapControllers();

app.Run();
