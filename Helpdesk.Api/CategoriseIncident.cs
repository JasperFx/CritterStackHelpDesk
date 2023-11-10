using Marten;
using Microsoft.AspNetCore.Mvc;

namespace Helpdesk.Api;

public record CategoriseIncident(
    Guid IncidentId,
    IncidentCategory Category,
    Guid UserId,
    int Version
);

public class CategoriseIncidentController : ControllerBase
{
    private readonly IDocumentSession _session;

    public CategoriseIncidentController(IDocumentSession session)
    {
        _session = session;
    }

    public async Task Post([FromBody] CategoriseIncident command)
    {
        // TODO -- validation on the input?
        // TODO -- trigger a prioritization if there is none?
        // TODO -- concurrency protection?
        
        // This code is definitely reused. Hmm.
        var userIdClaim = HttpContext.User.FindFirst("user-id");
        // It would probably help if we validate that this exists first,
        // and also that the user Id is actually a Guid. Later...
        
        var userId = Guid.Parse(userIdClaim.Value);

        var existing = await _session.Events.AggregateStreamAsync<IncidentDetails>(command.IncidentId);
        // What if the existing aggregate does not exist?
        
        if (existing.Category != command.Category)
        {
            var categorised = new IncidentCategorised(command.Category, userId);
            _session.Events.Append(command.IncidentId, categorised);
            await _session.SaveChangesAsync();
        }
    }
}