namespace BozoAIAggregator;

public class Configuration
{
    public int InfoPort { get; set; } = 8080;
    public string AzureOpenaiEndpoint { get; set; }
    public string AzureOpenaiKey { get; set; }
    public string AzureOpenChatDeploymentId { get; set; }
    public string AzureOpenEmbeddingsDeploymentId { get; set; }
}
