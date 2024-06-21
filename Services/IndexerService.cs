using Microsoft.Extensions.Options;
using Microsoft.KernelMemory;

namespace BozoAIAggregator.Services;

public class IndexerService : IHostedService
{
    private readonly ILogger<IndexerService> _logger;
    private readonly IOptions<Configuration> _configuration;
    private readonly Queue<Func<Task>> _indexQueue = new();
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
        _indexQueue.Enqueue(async () =>
        {
            await _kernelMemory.ImportWebPageAsync(uri.ToString());
        });
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
            await task();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _isRunning = false;
        return Task.CompletedTask;
    }
}
