﻿
using BozoAIAggregator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetCore.AutoRegisterDi;



var currentDirectory = Directory.GetCurrentDirectory();
var possibleAppSettingLocations = new[]
{
    Path.Combine(currentDirectory, "appsettings.json"),
    Path.Combine(currentDirectory, "..", "appsettings.json"),
    Path.Combine(currentDirectory, "..", "..", "appsettings.json"),
    Path.Combine(currentDirectory, "..", "..", "..", "appsettings.json"),
    Path.Combine(currentDirectory, "..", "..", "..", "..", "appsettings.json"),
    Path.Combine(currentDirectory, "..", "..", "..", "..", "..", "appsettings.json"),
    Path.Combine(currentDirectory, "..", "..", "..", "..", "..", "..", "appsettings.json"),
};




var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddEnvironmentVariables();


foreach (var possibleAppSettingLocation in possibleAppSettingLocations)
{
    if (File.Exists(possibleAppSettingLocation))
    {
        builder.Configuration.AddJsonFile(possibleAppSettingLocation, false, true);
        break;
    }
}

builder.Logging.AddConsole();
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Services.Configure<Configuration>(builder.Configuration);



// Register any class that ends with "Service" as a service
builder.Services.RegisterAssemblyPublicNonGenericClasses()
    .Where(c => c.Name.EndsWith("Service"))
    .AsPublicImplementedInterfaces();

var host = builder.Build();


await host.RunAsync();
