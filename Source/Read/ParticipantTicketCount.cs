using RaffleApplication.Events;

namespace RaffleApplication.Read;

[Projection("a1b2c3d4-e5f6-47a8-b9c0-1d2e3f4a5b6c")]
public class ParticipantTicketCount
{
    public string Email { get; set; } = "";
    public int TicketCount { get; set; }
    [KeyFromProperty(nameof(ParticipantRegistered.Email))]
    public void On(ParticipantRegistered evt, ProjectionContext _) => Email = evt.Email;
    [KeyFromProperty(nameof(ParticipantRegistered.Email))]
    public void On(SecretWordEntered evt, ProjectionContext _) => TicketCount++;
}