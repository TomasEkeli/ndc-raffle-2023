using Dolittle.SDK.Projections.Store;
using Microsoft.Extensions.Options;
using RaffleApplication.Read;

namespace RaffleApplication.Hosting;

[ApiController]
[Route("[controller]")]
public class RaffleController : ControllerBase
{
    readonly string _raffleId = "not-set";

    readonly IAggregateOf<Raffle> _raffles;
    readonly IProjectionStore _projections;
    readonly ILogger<RaffleController> _logger;

    public RaffleController(
        IOptions<RaffleOptions> options,
        IAggregateOf<Raffle> raffles,
        IProjectionStore projections,
        ILogger<RaffleController> logger)
    {
        _raffleId = options?.Value.Id ?? _raffleId;
        _raffles = raffles;
        _projections = projections;
        _logger = logger;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RegisterParticipant(
        [FromBody] RegisterParticipantRequest request)
    {
        try
        {
            await _raffles.Get(_raffleId)
                .Perform(raffle => raffle.RegisterParticipant(request.Email));
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error");
            return StatusCode(500);
        }
    }

    [HttpPost("secret")]
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
            .Get(new(_raffleId));

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
            .Get(new(_raffleId));

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

    [HttpGet("tickets")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTotalTicketCount()
    {
        var raffle = await _projections
            .Of<TotalTicketCount>()
            .Get(new(_raffleId));

        return Ok(raffle.TicketCount);
    }

    [HttpGet("id")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult GetRaffleId() => Ok(_raffleId);


    public record Winner(string Email, DateTimeOffset Timestamp);

    public record Participant(string Email, int Tickets);

    public record RegisterParticipantRequest(
        [Required, EmailAddress] string Email);

    public record EnterSecretWordRequest(
        [Required, EmailAddress] string Email,
        [Required] string SecretWord);
}