using Dolittle.SDK.Projections.Store;
using RaffleApplication.Read;

namespace RaffleApplication.Hosting;

[ApiController]
[Route("[controller]")]
public class RaffleController : ControllerBase
{
    public record RegisterParticipantRequest(
        [Required, EmailAddress] string Email);

    public record EnterSecretWordRequest(
        [Required, EmailAddress] string Email,
        [Required] string SecretWord);

    readonly IAggregateOf<Raffle> _raffles;
    readonly IProjectionOf<DrawHistory> _draws;
    readonly ILogger<RaffleController> _logger;

    public RaffleController(
        IAggregateOf<Raffle> raffles,
        IProjectionOf<DrawHistory> draws,
        ILogger<RaffleController> logger)
    {
        _raffles = raffles;
    _draws = draws;
    _logger = logger;
    }

    static readonly Guid _raffleId = new("3F2670BF-F382-4944-BA13-DB50C956CDEA");

    [HttpPost("register")]
    public async Task<IActionResult> RegisterParticipant(
        [FromBody] RegisterParticipantRequest request)
    {
        try
        {
            await _raffles.Get(_raffleId)
                .Perform(raffle => raffle.RegisterParticipant(request.Email));
            return Ok();
        }
        catch (AggregateException e)
        {
            return BadRequest(e.InnerException?.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error");
            return StatusCode(500);
        }
    }

    [HttpPost("enterSecretWord")]
    public async Task<IActionResult> EnterSecretWord([FromBody] EnterSecretWordRequest request)
    {
        try
        {
            await _raffles
                .Get(_raffleId)
                .Perform(raffle =>
                    raffle.EnterSecretWord(request.Email, request.SecretWord));
            return Ok();
        }
        catch (AggregateException e)
        {
            return BadRequest(e.InnerException?.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error");
            return StatusCode(500);
        }
    }

    [HttpPost("drawWinner")]
    public async Task<IActionResult> DrawWinner()
    {
        try
        {
            await _raffles.Get(_raffleId).Perform(raffle => raffle.DrawWinner());
            return Ok();
        }
        catch (AggregateException e)
        {
            return BadRequest(e.InnerException?.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error");
            return StatusCode(500);
        }
    }

    [HttpGet("winners")]
    public async Task<IActionResult> GetWinners()
    {
        var draws = await _draws.GetAll();

        return Ok(draws.OrderByDescending(_ => _.Timestamp));
    }
}