using Marten;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Helpdesk.Api;

public record CategoriseIncident(
    Guid IncidentId,
    IncidentCategory Category,
    Guid UserId,
    int Version
);

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
        
        var userId = Guid.Parse(userIdClaim.Value);

        return bus.InvokeAsync(command);
    }
}


public static class CategoriseIncidentHandler
{
    // This is a long hand version
    public static async Task Handle(
        CategoriseIncident command, 
        IDocumentSession session, 
        CancellationToken cancellationToken)
    {
        var existing = await session
            .Events
            .AggregateStreamAsync<IncidentDetails>(command.IncidentId, token: cancellationToken);

        if (existing == null) return;
        
        if (existing.Category != command.Category)
        {
            // Optimistic concurrency
            var expectedVersion = command.Version + 1;
            
            var categorised = new IncidentCategorised(command.Category, command.UserId);
            session.Events.Append(command.IncidentId, expectedVersion, categorised);
            await session.SaveChangesAsync(cancellationToken);
        }
    }
}