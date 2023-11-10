using Marten;
using Marten.Schema;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Marten;

namespace Helpdesk.Api;

public class CategoriseIncident
{
    [Identity]
    public Guid Id { get; set; }
    public IncidentCategory Category { get; set; }
    public Guid UserId { get; set; }
    public int Version { get; set; }
}

public class CategoriseIncidentController : ControllerBase
{
    public Task Post(
        [FromBody] CategoriseIncident command,
        [FromServices] IMessageBus bus)
    {
        // TODO -- validation on the input?
        // TODO -- trigger a prioritization if there is none?
        // TODO -- concurrency protection?
        
        // This code is definitely reused. Hmm.
        var userIdClaim = HttpContext.User.FindFirst("user-id");
        // It would probably help if we validate that this exists first,
        // and also that the user Id is actually a Guid. Later...

        command.UserId = Guid.Parse(userIdClaim.Value);

        return bus.InvokeAsync(command);
    }
}


public static class CategoriseIncidentHandler
{
    [AggregateHandler]
    public static IEnumerable<object> Handle(CategoriseIncident command, IncidentDetails existing)
    {
        if (existing.Category != command.Category)
        {
            yield return new IncidentCategorised(command.Category, command.UserId);
        }
    }
}