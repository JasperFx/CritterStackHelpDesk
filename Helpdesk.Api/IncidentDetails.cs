using Marten.Events;
using Marten.Events.Aggregation;

namespace Helpdesk.Api;

public record IncidentDetails(
    Guid Id,
    Guid CustomerId,
    IncidentStatus Status,
    IncidentNote[] Notes,
    IncidentCategory? Category = null,
    IncidentPriority? Priority = null,
    Guid? AgentId = null,
    int Version = 1
);

public record IncidentNote(
    IncidentNoteType Type,
    Guid From,
    string Content,
    bool VisibleToCustomer
);

public enum IncidentNoteType
{
    FromAgent,
    FromCustomer
}

public class IncidentDetailsProjection: SingleStreamProjection<IncidentDetails>
{
    public static IncidentDetails Create(IEvent<IncidentLogged> logged) =>
        new(logged.StreamId, logged.Data.CustomerId, IncidentStatus.Pending, Array.Empty<IncidentNote>());

    public IncidentDetails Apply(IncidentCategorised categorised, IncidentDetails current) =>
        current with { Category = categorised.Category };

    public IncidentDetails Apply(IncidentPrioritised prioritised, IncidentDetails current) =>
        current with { Priority = prioritised.Priority };

    public IncidentDetails Apply(AgentAssignedToIncident prioritised, IncidentDetails current) =>
        current with { AgentId = prioritised.AgentId };

    public IncidentDetails Apply(IncidentResolved resolved, IncidentDetails current) =>
        current with { Status = IncidentStatus.Resolved };

    public IncidentDetails Apply(ResolutionAcknowledgedByCustomer acknowledged, IncidentDetails current) =>
        current with { Status = IncidentStatus.ResolutionAcknowledgedByCustomer };

    public IncidentDetails Apply(IncidentClosed closed, IncidentDetails current) =>
        current with { Status = IncidentStatus.Closed };
}
