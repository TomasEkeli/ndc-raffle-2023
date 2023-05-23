using Dolittle.SDK.Projections.Store;
using RaffleApplication.Read;

namespace RaffleApplication.Hosting;

[ApiController]
[Route("[controller]")]
public class RaffleController : ControllerBase
{
    static readonly Guid _raffleId = new("3F2670BF-F382-4944-BA13-DB50C956CDEA");
    readonly IAggregateOf<Raffle> _raffles;
    readonly IProjectionStore _projections;
    readonly ILogger<RaffleController> _logger;

    public RaffleController(
        IAggregateOf<Raffle> raffles,
        IProjectionStore projections,
        ILogger<RaffleController> logger)
    {
        _raffles = raffles;
        _projections = projections;
        _logger = logger;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterParticipant(
        [FromBody] RegisterParticipantRequest request)
    {
        try
        {
            await _raffles.Get(_raffleId)
                .Perform(raffle => raffle.RegisterParticipant(request.Email));
            return Ok();
        }
        catch (AggregateException)
        {
            return BadRequest("already registered");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error");
            return StatusCode(500);
        }
    }

    [HttpPost("enterSecretWord")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EnterSecretWord(
        [FromBody] EnterSecretWordRequest request)
    {
        try
        {
            await _raffles
                .Get(_raffleId)
                .Perform(raffle =>
                    raffle.EnterSecretWord(request.Email, request.SecretWord));
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error");
            return StatusCode(500);
        }
    }

    [HttpPost("draw")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DrawWinner()
    {
        try
        {
            await _raffles.Get(_raffleId).Perform(raffle => raffle.DrawWinner());
            return Ok();
        }
        catch (AggregateRootOperationFailed)
        {
            return BadRequest("No tickets entered");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error");
            return StatusCode(500);
        }
    }

    [HttpGet("winners")]
    [ProducesResponseType(typeof(Winner[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWinners()
    {
        var raffle = await _projections
            .Of<DrawHistory>()
            .Get(new(_raffleId.ToString()));

        if (!raffle.Winners.Any())
        {
            return NotFound();
        }

        return Ok(
            raffle
                .Winners
                .Select(kv => new Winner(kv.Value, kv.Key))
                .OrderByDescending(_ => _.Timestamp));
    }

    [HttpGet("participants")]
    [ProducesResponseType(typeof(Participant[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetParticipants()
    {
        var raffle = await _projections
            .Of<ParticipantsTicketCount>()
            .Get(new Dolittle.SDK.Projections.Key(_raffleId.ToString()));

        if (!raffle.Participants.Any())
        {
            return NotFound();
        }

        return Ok(
                raffle
                    .Participants
                    .Select(kv => new Participant(kv.Key, kv.Value))
                    .OrderByDescending(_ => _.Tickets));
    }

    public record Winner(string Email, DateTimeOffset Timestamp);

    public record Participant(string Email, int Tickets);

    public record RegisterParticipantRequest(
        [Required, EmailAddress] string Email);

    public record EnterSecretWordRequest(
        [Required, EmailAddress] string Email,
        [Required] string SecretWord);
}