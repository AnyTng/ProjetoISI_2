using Microsoft.EntityFrameworkCore;
using SmartParking_Api.Data;


var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("AzureBDConnection");


// 1) Connection string vinda dos user-secrets


// 2) DbContext com SQL Server
builder.Services.AddDbContext<SmartParkingDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3) REST + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();