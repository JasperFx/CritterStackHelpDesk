using Marten.Events;
using Marten.Schema;

namespace Helpdesk.Api;

public record IncidentLogged(
    Guid CustomerId,
    Contact Contact,
    string Description,
    Guid LoggedBy
);

// Some hacking here that will hopefully be eliminated by Monday. Gulp.
public class IncidentCategorised
{
    public IncidentCategory Category { get; set; }
    public Guid UserId { get; set; }
}

public record IncidentPrioritised(IncidentPriority Priority, Guid UserId);

public record AgentAssignedToIncident(Guid AgentId);

public record AgentRespondedToIncident(        
    Guid AgentId,
    string Content,
    bool VisibleToCustomer);

public record CustomerRespondedToIncident(
    Guid UserId,
    string Content
);

public record IncidentResolved(
    ResolutionType Resolution,
    Guid ResolvedBy,
    DateTimeOffset ResolvedAt
);

public record ResolutionAcknowledgedByCustomer(
    Guid IncidentId,
    Guid AcknowledgedBy,
    DateTimeOffset AcknowledgedAt
);

public record IncidentClosed(
    Guid IncidentId,
    Guid ClosedBy,
    DateTimeOffset ClosedAt
);

public enum IncidentStatus
{
    Pending = 1,
    Resolved = 8,
    ResolutionAcknowledgedByCustomer = 16,
    Closed = 32
}

public record Incident(
    Guid Id,
    IncidentStatus Status,
    bool HasOutstandingResponseToCustomer = false
)
{
    public static Incident Create(IEvent<IncidentLogged> logged) =>
        new(logged.Id, IncidentStatus.Pending);

    public Incident Apply(AgentRespondedToIncident agentResponded) =>
        this with { HasOutstandingResponseToCustomer = false };

    public Incident Apply(CustomerRespondedToIncident customerResponded) =>
        this with { HasOutstandingResponseToCustomer = true };

    public Incident Apply(IncidentResolved resolved) =>
        this with { Status = IncidentStatus.Resolved };

    public Incident Apply(ResolutionAcknowledgedByCustomer acknowledged) =>
        this with { Status = IncidentStatus.ResolutionAcknowledgedByCustomer };

    public Incident Apply(IncidentClosed closed) =>
        this with { Status = IncidentStatus.Closed };
}

public enum IncidentCategory
{
    Software,
    Hardware,
    Network,
    Database
}

public enum IncidentPriority
{
    Critical,
    High,
    Medium,
    Low
}

public enum ResolutionType
{
    Temporary,
    Permanent,
    NotAnIncident
}

public enum ContactChannel
{
    Email,
    Phone,
    InPerson,
    GeneratedBySystem
}

public record Contact(
    ContactChannel ContactChannel,
    string? FirstName = null,
    string? LastName = null,
    string? EmailAddress = null,
    string? PhoneNumber = null
);


