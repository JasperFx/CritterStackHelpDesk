using Marten;
using Wolverine;
using Wolverine.Marten;

namespace Helpdesk.Api;

public record RingAllTheAlarms(Guid IncidentId);

public static class IncidentCategorisedHandler
{
    public static Task<Customer?> LoadAsync(IncidentDetails details, IDocumentSession session)
    {
        return session.LoadAsync<Customer>(details.CustomerId);
    }

    [AggregateHandler]
    public static (Events, OutgoingMessages) Handle(IncidentCategorised categorised, IncidentDetails details,
        Customer customer)
    {
        var events = new Events();
        var messages = new OutgoingMessages();
        
        if (customer.Priorities.TryGetValue(categorised.Category, out var priority))
        {
            if (details.Priority != priority)
            {
                events.Add(new IncidentPrioritised(priority, categorised.UserId));

                if (priority == IncidentPriority.Critical)
                {
                    messages.Add(new RingAllTheAlarms(categorised.IncidentId));
                }
            }
        }

        return (events, messages);
    }
}