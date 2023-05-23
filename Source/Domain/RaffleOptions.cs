using System.ComponentModel.DataAnnotations;

namespace RaffleApplication.Domain;

public class RaffleOptions
{
    public const string SectionName = "Raffle";

    [Required]
    public string Id { get; set; } = string.Empty;

    [Required]
    public string[] SecretWords { get; set; } = Array.Empty<string>();

    [Range(1, 100)]
    public int NumberOfTicketsEach { get; set; } = 1;
}
