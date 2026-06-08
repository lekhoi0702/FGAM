using Microsoft.EntityFrameworkCore;
using WarehouseAccessAPI.Models;
using WarehouseAccessAPI.Common;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);
const string FrontendCorsPolicy = "FrontendCorsPolicy";

builder.Services.AddDbContext<FgamContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection"));
});
builder.Services.AddScoped<WarehouseAccessAPI.Services.AuthService>();
builder.Services.AddScoped<WarehouseAccessAPI.Services.EmployeeService>();
builder.Services.AddScoped<WarehouseAccessAPI.Services.FactoryService>();
builder.Services.AddScoped<WarehouseAccessAPI.Services.DepartmentService>();
builder.Services.AddScoped<WarehouseAccessAPI.Services.TransactionService>();
builder.Services.Configure<GuestApi>(
    builder.Configuration.GetSection(GuestApi.SectionName));
builder.Services.AddHttpClient();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString;
    });
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "Warehouse Access API", 
        Version = "v1" 
    });
});
var app = builder.Build();

app.UsePathBase("/WarehouseAccessAPi");

app.UseSwagger(c =>
{
    c.RouteTemplate = "swagger/{documentName}/swagger.json";
});
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("v1/swagger.json", "Warehouse Access API");
    c.RoutePrefix = "swagger";
});

app.UseCors(FrontendCorsPolicy);

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();
app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();

