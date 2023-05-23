namespace RaffleApplication.Domain;

public class InvalidSecretWordException : Exception
{
    public InvalidSecretWordException(string message) : base(message)
    {
    }
}