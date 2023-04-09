using System.ComponentModel.DataAnnotations;

namespace Mimir.Api.Configurations;

public class ChatGptOptions
{
    public const string Key = "ChatGpt";
    
    [Required(AllowEmptyStrings = false)]
    public string ApiDomain { get; set; } = string.Empty;
    
    [Required(AllowEmptyStrings = false)]
    public string ApiKey { get; set; } = string.Empty;
}