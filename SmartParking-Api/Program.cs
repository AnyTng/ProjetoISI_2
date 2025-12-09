using Microsoft.EntityFrameworkCore;
using SoapCore;
using SmartParking_Api.Data;
using SmartParking_Api.Services;
using SmartParking_Api.Services.Soap;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("AzureBDConnection");

// DbContext (Azure SQL)
builder.Services.AddDbContext<SmartParkingDbContext>(options =>
    options.UseSqlServer(connectionString));

// Weather (Open-Meteo)
builder.Services.AddHttpClient<IWeatherService, OpenMeteoWeatherService>();

// SOAP
builder.Services.AddSoapCore();
builder.Services.AddScoped<ISmartParkingSoapService, SmartParkingSoapService>();

// REST + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


// SOAP endpoint
var routes = (IEndpointRouteBuilder)app;

routes.UseSoapEndpoint<ISmartParkingSoapService>(
    "/soap/smartparking.asmx",
    new SoapEncoderOptions(),
    SoapSerializer.XmlSerializer);

// REST controllers
app.MapControllers();

app.Run();