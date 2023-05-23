namespace RaffleApplication.Domain;

public class ParticipantAlreadyRegisteredException : Exception
{
    public ParticipantAlreadyRegisteredException(string message) : base(message)
    {
    }
}