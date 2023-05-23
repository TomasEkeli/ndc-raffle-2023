namespace RaffleApplication.Domain;

[AggregateRoot("283df303-ecbd-41f8-af59-889c94279ac9")]
public class Raffle : AggregateRoot
{
    readonly HashSet<string> _participants = new();
    readonly HashSet<(string, string)> _words = new();
    readonly Dictionary<string, int> _ticketCounts = new();
    int _totalTickets;
    readonly string[] _secretWords =
    {
        "dummy1",
        "dummy2",
        "dummy3"
    };
    public void RegisterParticipant(string email)
    {
        if (_participants.Contains(email))
        {
            throw new ParticipantAlreadyRegisteredException("This email is already registered.");
        }

        Apply(new ParticipantRegistered(email));
    }

    public void EnterSecretWord(string email, string secretWord)
    {
        if (!_participants.Contains(email))
        {
            throw new ParticipantNotRegisteredException("This email is not registered.");
        }

        if (!_secretWords.Contains(secretWord))
        {
            throw new InvalidSecretWordException("The secret word is not valid.");
        }

        if (_words.Contains((email, secretWord)))
        {
            throw new InvalidSecretWordException("No double-dipping.");
        }

        if (_ticketCounts[email] >= 3)
        {
            throw new MaximumTicketsReachedException(
                "The maximum number of tickets has been reached.");
        }

        Apply(new SecretWordEntered(email, secretWord));
    }

    public void DrawWinner()
    {
        if (_totalTickets == 0)
        {
            throw new NoTicketsException("There are no tickets to draw from.");
        }

        var winnerEmail = PickWinner();
        Apply(new WinnerDrawn(DateTimeOffset.UtcNow, winnerEmail));
    }

    void On(ParticipantRegistered evt)
    {
        _participants.Add(evt.Email);
        _ticketCounts[evt.Email] = 0;
    }

    void On(SecretWordEntered evt)
    {
        _ticketCounts[evt.Email]++;
        _words.Add((evt.Email, evt.SecretWord));
        _totalTickets++;
    }

    void On(WinnerDrawn evt)
    {
    }

    public string PickWinner()
    {
        var random = new Random();
        var winnerIndex = random.Next(_totalTickets);
        var currentIndex = 0;

        foreach (var participant in _ticketCounts)
        {
            currentIndex += participant.Value;
            if (currentIndex > winnerIndex)
            {
                return participant.Key;
            }
        }

        throw new InvalidOperationException("Failed to pick a winner.");
    }
}