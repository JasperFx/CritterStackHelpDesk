using Marten;
using Marten.Events;
using Marten.Schema;
using Wolverine;
using Wolverine.Marten;

namespace Helpdesk.Api;

public record RingAllTheAlarms(Guid IncidentId);

public class TryAssignPriority
{
    [Identity]
    public Guid IncidentId { get; set; }
    public Guid UserId { get; set; }
}

public static class TryAssignPriorityHandler
{
    public static Task<Customer?> LoadAsync(IncidentDetails details, IDocumentSession session)
    {
        return session.LoadAsync<Customer>(details.CustomerId);
    }

    [AggregateHandler]
    public static (Events, OutgoingMessages) Handle(TryAssignPriority command, IncidentDetails details,
        Customer customer)
    {
        var events = new Events();
        var messages = new OutgoingMessages();

        if (details.Category.HasValue && customer.Priorities.TryGetValue(details.Category.Value, out var priority))
        {
            if (details.Priority != priority)
            {
                events.Add(new IncidentPrioritised(priority, command.UserId));

                if (priority == IncidentPriority.Critical)
                {
                    messages.Add(new RingAllTheAlarms(command.IncidentId));
                }
            }
        }

        return (events, messages);
    }
}