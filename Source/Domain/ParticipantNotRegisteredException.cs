namespace RaffleApplication.Domain;

public class ParticipantNotRegisteredException : Exception
{
    public ParticipantNotRegisteredException(string message) : base(message)
    {
    }
}