using System.ComponentModel.DataAnnotations;

namespace Mimir.Infrastructure.Configurations;

public class DynamoDbOptions
{
    public const string Key = "DynamoDB";
    
    [Required(AllowEmptyStrings = false)]
    public string TableName { get; set; } = string.Empty;
}