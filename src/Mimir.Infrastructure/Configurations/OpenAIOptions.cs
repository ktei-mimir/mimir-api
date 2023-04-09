using System.ComponentModel.DataAnnotations;

namespace Mimir.Infrastructure.Configurations;

public class OpenAIOptions
{
    public const string Key = "OpenAI";
    
    [Required(AllowEmptyStrings = false)]
    public string ApiDomain { get; set; } = string.Empty;
    
    [Required(AllowEmptyStrings = false)]
    public string ApiKey { get; set; } = string.Empty;
}