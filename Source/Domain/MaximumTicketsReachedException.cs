namespace RaffleApplication.Domain;

public class MaximumTicketsReachedException : Exception
{
    public MaximumTicketsReachedException(string message) : base(message)
    {
    }
}