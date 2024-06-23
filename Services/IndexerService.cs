using System.Runtime.Serialization;
using Microsoft.Extensions.Options;
using Microsoft.KernelMemory;

namespace BozoAIAggregator.Services;

public class IndexerService : IHostedService
{
    private readonly ILogger<IndexerService> _logger;
    private readonly IOptions<Configuration> _configuration;
    private readonly Queue<TaskSerializable> _indexQueue = new();
    private readonly IKernelMemory _kernelMemory;
    private bool _isRunning;
    public IndexerService(ILogger<IndexerService> logger, IOptions<Configuration> configuration, IKernelMemory kernelMemory)
    {
        _logger = logger;
        _configuration = configuration;
        _kernelMemory = kernelMemory;
    }

    public void AddWebScrappingTask(Uri uri)
    {
        _indexQueue.Enqueue(new WebScrappingTask(uri));
    }


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _isRunning = true;
        while (_isRunning)
        {
            if (_indexQueue.Count == 0)
            {
                _logger.LogInformation("IndexerService: No tasks in queue");
                await Task.Delay(1000, cancellationToken);
                continue;
            }

            var task = _indexQueue.Dequeue();
            switch (task)
            {
                case WebScrappingTask webScrappingTask:
                    _logger.LogInformation($"IndexerService: Processing WebScrappingTask for {webScrappingTask.Uri}");
                    await _kernelMemory.ImportWebPageAsync(webScrappingTask.Uri.ToString(),
                        cancellationToken: cancellationToken);
                    break;
                default:
                    _logger.LogError($"IndexerService: Unknown task type {task.TaskType}");
                    break;
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _isRunning = false;
        return Task.CompletedTask;
    }

    private abstract class TaskSerializable : ISerializable
    {
        public string TaskType { get; }


        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("TaskType", TaskType);

        }
    }

    private class WebScrappingTask : TaskSerializable
    {
        public Uri Uri { get; set; }
        public string TaskType => "WebScrappingTask";
        public WebScrappingTask(Uri uri)
        {
            Uri = uri;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Uri", Uri);
        }
    }
}
