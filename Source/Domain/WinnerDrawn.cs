namespace RaffleApplication.Domain;

[EventType("495eb7c2-32b2-4540-98ab-100ed1865cb5")]
public record WinnerDrawn(DateTimeOffset Timestamp, string WinnerEmail);