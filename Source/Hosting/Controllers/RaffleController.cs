using Dolittle.SDK.Projections.Store;
using RaffleApplication.Read;

namespace RaffleApplication.Hosting;

public record RegisterParticipantRequest(
    [Required, EmailAddress] string Email);

public record EnterSecretWordRequest(
    [Required, EmailAddress] string Email,
    [Required] string SecretWord);

[ApiController]
[Route("[controller]")]
public class RaffleController : ControllerBase
{
    static readonly Guid _raffleId = new("3F2670BF-F382-4944-BA13-DB50C956CDEA");
    readonly IAggregateOf<Raffle> _raffles;
    readonly IProjectionOf<DrawHistory> _draws;
    readonly IProjectionOf<ParticipantTicketCount> _participants;
    readonly ILogger<RaffleController> _logger;

    public RaffleController(
        IAggregateOf<Raffle> raffles,
        IProjectionOf<DrawHistory> draws,
        // IProjectionOf<ParticipantTicketCount> participants,
        ILogger<RaffleController> logger)
    {
        _raffles = raffles;
        _draws = draws;
        // _participants = participants;
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
        catch (AggregateException)
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWinners()
    {
        var draws = await _draws.GetAll();

        if (!draws.Any())
        {
            return NotFound();
        }

        return Ok(draws.OrderByDescending(_ => _.Timestamp));
    }

    [HttpGet("participants")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetParticipants()
    {
        var raffle = await _participants.GetAll();

        if (!raffle.Any())
        {
            return NotFound();
        }

        return Ok(raffle.OrderByDescending(_ => _.TicketCount));
    }
}