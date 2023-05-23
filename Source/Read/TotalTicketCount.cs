namespace RaffleApplication.Read;

[Projection("b2c3d4e5-f6a7-48b9-c0d1-2e3f4a5b6c7d")]
public class TotalTicketCount
{
    public int TicketCount { get; set; }

    [KeyFromEventSource]
    public void On(SecretWordEntered evt, ProjectionContext _) => TicketCount++;
}