namespace RaffleApplication.Read;

[Projection("FF556BDA-3F2F-453B-99DB-4B5775DCBA25")]
public class DrawHistory
{
    public DateTimeOffset Timestamp { get; set; }

    public string WinnerEmail { get; set; } = "";

    [KeyFromProperty(nameof(WinnerDrawn.Timestamp))]
    public void On(WinnerDrawn evt, ProjectionContext ctx)
    {
        Timestamp = evt.Timestamp;
        WinnerEmail = evt.WinnerEmail;
    }
}