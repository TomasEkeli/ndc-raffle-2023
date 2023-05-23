namespace RaffleApplication.Domain;

[EventHandler("acd7f76c-1e4c-4571-803c-1082a8fd76b9")]
public class RaffleHandler
{
    readonly IAggregateOf<Raffle> _raffles;
    readonly ILogger<RaffleHandler> _logger;
    public RaffleHandler(IAggregateOf<Raffle> raffles, ILogger<RaffleHandler> logger)
    {
        _raffles = raffles;
        _logger = logger;
    }

    public void Handle(ParticipantRegistered evt, EventContext ctx)
    {
        _logger.LogInformation("{Email} registered for the raffle at {DateTimeOffset}", evt.Email, ctx.Occurred);
    }

    public void Handle(SecretWordEntered evt, EventContext ctx)
    {
        _logger.LogInformation("{Email} entered secret word {SecretWord} at {DateTimeOffset}", evt.Email, evt.SecretWord, ctx.Occurred);
    }

    public void Handle(WinnerDrawn evt, EventContext ctx)
    {
        _logger.LogInformation("Winner {WinnerEmail} drawn at {DateTimeOffset}", evt.WinnerEmail, evt.Timestamp);
    }
}