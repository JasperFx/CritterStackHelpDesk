using JasperFx.Core;
using Marten;
using Microsoft.AspNetCore.Mvc;

namespace Helpdesk.Api;

public record LogIncident(
    Guid IncidentId,
    Guid CustomerId,
    Contact Contact,
    string Description
);

public class LogIncidentController : ControllerBase
{
    private readonly IDocumentSession _session;

    public LogIncidentController(IDocumentSession session)
    {
        _session = session;
    }

    // TODO -- add some OpenAPI metadata
    [HttpPost("/api/incidents")]
    public async Task<IResult> Post([FromBody] LogIncident command)
    {
        // TODO -- validation on the input?
        // TODO -- should we maybe try to prioritize this upfront?
        // TODO -- should we categorize this somehow?
        
        var userIdClaim = HttpContext.User.FindFirst("user-id");
        // It would probably help if we validate that this exists first,
        // and also that the user Id is actually a Guid. Later...
        
        var userId = Guid.Parse(userIdClaim.Value);

        var logged = new IncidentLogged(command.CustomerId, command.Contact, command.Description, userId);

        _session.Events.StartStream<Incident>(command.IncidentId, logged);

        await _session.SaveChangesAsync();
        
        return Results.Created($"/api/incidents/{command.IncidentId}", command.IncidentId);
    }     
}