
using System.Net;
using Azure;
using Azure.AI.OpenAI;
using Azure.Core.Pipeline;
using BozoAIAggregator;
using BozoAIAggregator.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Neo4j.KernelMemory.MemoryStorage;
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




var builder = WebApplication.CreateBuilder(args);
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
builder.Services.AddDbContext<DatabaseContext>();
var azureOpenaiEndpoint = builder.Configuration["AzureOpenaiEndpoint"];
var azureOpenaiKey = builder.Configuration["AzureOpenaiKey"];
var azureOpenChatDeploymentId = builder.Configuration["AzureOpenChatDeploymentId"];
var azureOpenEmbeddingsDeploymentId = builder.Configuration["AzureOpenEmbeddingsDeploymentId"];
if(string.IsNullOrEmpty(azureOpenaiEndpoint) || string.IsNullOrEmpty(azureOpenaiKey) || string.IsNullOrEmpty(azureOpenChatDeploymentId) || string.IsNullOrEmpty(azureOpenEmbeddingsDeploymentId))
{
    throw new Exception("AzureOpenaiEndpoint, AzureOpenaiKey, AzureOpenChatDeploymentId, and AzureOpenEmbeddingsDeploymentId must be set in the configuration.");
}
// Create an HttpClient and include your custom header(s)
var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Add("My-Custom-Header", "My Custom Value");

// Configure OpenAIClient to use the customized HttpClient
var clientOptions = new OpenAIClientOptions
{
    Transport = new HttpClientTransport(httpClient),
};
var openAiClient = new OpenAIClient(new Uri(azureOpenaiEndpoint), new AzureKeyCredential(azureOpenaiKey), clientOptions);

builder.Services.AddAzureOpenAIChatCompletion(azureOpenChatDeploymentId, openAiClient);
#pragma warning disable SKEXP0001, SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future releases.
builder.Services.AddAzureOpenAITextEmbeddingGeneration(azureOpenEmbeddingsDeploymentId, openAiClient);


builder.AddKernelMemory(kmBuilder =>
    {
        // Configure Kernel Memory here if needed
        kmBuilder
            .WithNeo4j(new Neo4jConfig()
            {
                Uri = builder.Configuration["Neo4jEndpoint"] ?? throw new Exception("Neo4jEndpoint must be set in the configuration."),
                Username = builder.Configuration["Neo4jUser"] ?? throw new Exception("Neo4jUser must be set in the configuration."),
                Password = builder.Configuration["Neo4jPassword"] ?? throw new Exception("Neo4jPassword must be set in the configuration.")
            })

            .WithAzureOpenAITextEmbeddingGeneration(new AzureOpenAIConfig()
            {
                APIKey = azureOpenaiKey,
                APIType = AzureOpenAIConfig.APITypes.EmbeddingGeneration,
                Auth = AzureOpenAIConfig.AuthTypes.APIKey,
                Endpoint = azureOpenaiEndpoint,
                Deployment = azureOpenEmbeddingsDeploymentId
            })
            .WithAzureOpenAITextGeneration(new AzureOpenAIConfig()
            {
                APIKey = azureOpenaiKey,
                APIType = AzureOpenAIConfig.APITypes.ChatCompletion,
                Auth = AzureOpenAIConfig.AuthTypes.APIKey,
                Endpoint = azureOpenaiEndpoint,
                Deployment = azureOpenChatDeploymentId
            });
    }
);

// Register any class that ends with "Service" as a service
builder.Services.RegisterAssemblyPublicNonGenericClasses()
    .Where(c => c.Name.EndsWith("Service"))
    .AsPublicImplementedInterfaces(ServiceLifetime.Singleton);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
