using System.ComponentModel.DataAnnotations;

namespace BozoAIAggregator.Models;

public class Status
{
    public int Id { get; set; }
    public string Content { get; set; }
    [DataType(DataType.Date)]
    public DateTime CreatedAt { get; set; }
}
