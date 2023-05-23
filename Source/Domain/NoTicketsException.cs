namespace RaffleApplication.Domain;

public class NoTicketsException : Exception
{
    public NoTicketsException(string message) : base(message)
    {
    }
}