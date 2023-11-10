using Marten;
using Marten.AspNetCore;
using Wolverine.Http;

namespace Helpdesk.Api;

public static class GetIncident
{
    [WolverineGet("/incident/{id}")]
    public static Task Get(Guid id, IQuerySession session, HttpContext context)
    {
        return session.Json.WriteById<IncidentDetails>(id, context);
    }
}