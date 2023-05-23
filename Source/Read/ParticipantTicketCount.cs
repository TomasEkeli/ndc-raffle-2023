using RaffleApplication.Events;

namespace RaffleApplication.Read;

[Projection("8BEC3F4E-EE62-40A6-9528-899E9DFF4BDD")]
public class ParticipantsTicketCount
{
    public Dictionary<string, int> Participants { get; set; } = new();
    public int TicketCount { get; set; }

    [KeyFromEventSource]
    public void On(ParticipantRegistered evt, ProjectionContext _) =>
        Participants.Add(evt.Email, 1);

    [KeyFromEventSource]
    public void On(SecretWordEntered evt, ProjectionContext _) =>
        Participants[evt.Email] = Participants[evt.Email] + 1;
}