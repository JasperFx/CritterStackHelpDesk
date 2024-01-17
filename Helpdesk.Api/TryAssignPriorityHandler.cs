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
    // Wolverine will call this method before the "real" Handler method,
    // and it can "magically" connect that the Customer object should be delivered
    // to the Handle() method at runtime
    public static Task<Customer?> LoadAsync(IncidentDetails details, IDocumentSession session)
    {
        return session.LoadAsync<Customer>(details.CustomerId);
    }

    // There's some database lookup at runtime, but I've isolated that above, so the
    // behavioral logic that "decides" what to do is a pure function below. 
    [AggregateHandler]
    public static (Events, OutgoingMessages) Handle(
        TryAssignPriority command, 
        IncidentDetails details,
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