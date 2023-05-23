using RaffleApplication.Events;

namespace RaffleApplication.Domain.Tests;

public class RaffleTests : AggregateRootTests<Raffle>
{
    const string Email1 = "participant1@example.com";
    const string Email2 = "participant2@example.com";
    const string SecretWord1 = "dummy1";
    const string SecretWord2 = "dummy2";
    const string SecretWord3 = "dummy3";
    public RaffleTests() : base(_ => new Raffle(), Guid.NewGuid().ToString())
    {
    }

    [Fact]
    public void Should_register_participant()
    {
        WhenPerforming(raffle => raffle.RegisterParticipant(Email1));
        AssertThat.ShouldHaveNumberOfEvents(1);
        AssertThat.ShouldHaveEvent<ParticipantRegistered>().First().Where(evt => evt.Email.Should().Be(Email1));
    }

    // [Fact]
    // public void Should_throw_when_registering_already_registered_participant()
    // {
    //     WithAggregateInState(raffle => raffle.RegisterParticipant(Email1));
    //     Aggregate.Invoking(agg => agg.Perform(raffle => raffle.RegisterParticipant(Email1))).Should().ThrowExactlyAsync<ParticipantAlreadyRegisteredException>();
    // }

    [Fact]
    public void Should_enter_secret_word()
    {
        WithAggregateInState(raffle => raffle.RegisterParticipant(Email1));
        WhenPerforming(raffle => raffle.EnterSecretWord(Email1, SecretWord1));
        AssertThat.ShouldHaveNumberOfEvents(1);
        AssertThat.ShouldHaveEvent<SecretWordEntered>().First().Where(evt =>
        {
            evt.Email.Should().Be(Email1);
            evt.SecretWord.Should().Be(SecretWord1);
        });
    }

    // [Fact]
    // public void Should_throw_when_entering_secret_word_for_unregistered_participant()
    // {
    //     Aggregate.Invoking(agg => agg.Perform(raffle => raffle.EnterSecretWord(Email1, SecretWord1))).Should().ThrowExactlyAsync<ParticipantNotRegisteredException>();
    // }

    // [Fact]
    // public void Should_throw_when_entering_invalid_secret_word()
    // {
    //     WithAggregateInState(raffle => raffle.RegisterParticipant(Email1));
    //     Aggregate.Invoking(agg => agg.Perform(raffle => raffle.EnterSecretWord(Email1, "invalid"))).Should().ThrowExactlyAsync<InvalidSecretWordException>();
    // }

    // [Fact]
    // public void Should_throw_when_entering_more_than_three_secret_words()
    // {
    //     WithAggregateInState(raffle =>
    //     {
    //         raffle.RegisterParticipant(Email1);
    //         raffle.EnterSecretWord(Email1, SecretWord1);
    //         raffle.EnterSecretWord(Email1, SecretWord2);
    //         raffle.EnterSecretWord(Email1, SecretWord3);
    //     });
    //     Aggregate.Invoking(agg => agg.Perform(raffle => raffle.EnterSecretWord(Email1, SecretWord1))).Should().ThrowExactlyAsync<MaximumTicketsReachedException>();
    // }

    [Fact]
    public void Should_draw_winner()
    {
        WithAggregateInState(raffle =>
        {
            raffle.RegisterParticipant(Email1);
            raffle.EnterSecretWord(Email1, SecretWord1);
            raffle.RegisterParticipant(Email2);
            raffle.EnterSecretWord(Email2, SecretWord2);
        });
        WhenPerforming(raffle => raffle.DrawWinner());
        AssertThat.ShouldHaveNumberOfEvents(1);
        AssertThat.ShouldHaveEvent<WinnerDrawn>().First().Where(evt =>
        {
            evt.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
            (evt.WinnerEmail == Email1 || evt.WinnerEmail == Email2).Should().BeTrue();
        });
    }

    [Fact]
    public void Should_throw_when_drawing_winner_with_no_tickets()
    {
        Aggregate.Invoking(agg => agg.Perform(raffle => raffle.DrawWinner())).Should().ThrowExactlyAsync<NoTicketsException>();
    }
}