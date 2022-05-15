using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using walkwards_api.Notifications;
using walkwards_api.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = true);
builder.Services.AddControllers().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// Console.Write(await NotificationsManager.GetAllDevices());
// Console.Write(await NotificationsManager.SendNotifications("online-msg", "Jesteśmy spowrotem!!!", "Nasze serwery są znowu dostępne",
//     null,
//     new string[] {"643500", "000000"})); 

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseCors();

app.MapControllers();

await LoggerManager.WriteLog("Server started");

app.Run();
