using System.ComponentModel.DataAnnotations;

namespace OPI.WebUI.ViewModels;

public class BatchContent
{
    [Required]
    public string? Value { get; set; } = default;
}