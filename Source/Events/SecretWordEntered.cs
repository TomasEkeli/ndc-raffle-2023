namespace RaffleApplication.Events;

[EventType("54638bf8-9727-4d88-92c3-14ca2b1b7d65")]
public record SecretWordEntered(string Email, string SecretWord);