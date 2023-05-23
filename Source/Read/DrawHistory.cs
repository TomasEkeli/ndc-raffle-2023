using RaffleApplication.Events;

namespace RaffleApplication.Read;

[Projection("FF556BDA-3F2F-453B-99DB-4B5775DCBA25")]
public class DrawHistory
{
    public Dictionary<DateTimeOffset, string> Winners { get; set; } = new();

    [KeyFromEventSource]
    public void On(WinnerDrawn evt, ProjectionContext ctx)
    {
        Winners.Add(evt.Timestamp, evt.WinnerEmail);
    }
}