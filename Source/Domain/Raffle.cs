using RaffleApplication.Events;

namespace RaffleApplication.Domain;

[AggregateRoot("283df303-ecbd-41f8-af59-889c94279ac9")]
public class Raffle : AggregateRoot
{
    const int _maxNumberOfTickets = 3;
    readonly HashSet<string> _participants = new();
    readonly HashSet<(string, string)> _words = new();
    readonly Dictionary<string, int> _ticketCounts = new();
    int _totalTickets;
    readonly string[] _secretWords =
    {
        /*
         Representing the vast and interconnected nature of event-driven
         systems, Nebula signifies the distributed and dynamic nature of
         events in an architectural context.
        */
        "nebula",
        /*
         Signifying the connections and neural pathways of the brain, Synapse
         relates to the integration of AI technologies within event-driven
         systems, highlighting the intelligent decision-making capabilities.
        */
        "synapse",
        /*
         Evoking the concept of guardianship and vigilance, Sentinel represents
         the monitoring and detection mechanisms within event-driven systems
         that leverage AI capabilities to ensure proactive and responsive actions.
        */
        "sentinel"
    };
    public void RegisterParticipant(string email)
    {
        if (!_participants.Contains(email))
        {
            Apply(new ParticipantRegistered(email));
        }
    }

    public void EnterSecretWord(string email, string secretWord)
    {
        secretWord = secretWord.ToLowerInvariant();

        if (!_participants.Contains(email))
        {
            RegisterParticipant(email);
        }

        if (!_secretWords.Contains(secretWord))
        {
            //The secret word is not valid.
            return;
        }

        if (_words.Contains((email, secretWord)))
        {
            // No double-dipping
            return;
        }

        if (_ticketCounts[email] >= _maxNumberOfTickets)
        {
            // The maximum number of tickets has been reached.
            return;
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

    string PickWinner()
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

    void On(WinnerDrawn _)
    {
    }

}