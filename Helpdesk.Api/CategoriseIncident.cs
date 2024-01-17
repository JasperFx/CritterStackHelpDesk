using System.Diagnostics;
using System.Text.Json.Serialization;
using FluentValidation;
using Marten;
using Marten.Schema;
using Wolverine;
using Wolverine.Http;
using Wolverine.Marten;

namespace Helpdesk.Api;

public class CategoriseIncident
{
    [Identity, JsonPropertyName("Id")]
    public Guid Id { get; set; }
    
    [JsonPropertyName("Category")]
    public IncidentCategory Category { get; set; }
    
    [JsonPropertyName("Version")]
    public int Version { get; set; }
    
    public class CategoriseIncidentValidator : AbstractValidator<CategoriseIncident>
    {
        public CategoriseIncidentValidator()
        {
            RuleFor(x => x.Version).GreaterThan(0);
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}

public static class CategoriseIncidentEndpoint
{
    [WolverinePost("/api/incidents/categorise"), AggregateHandler]
    public static (Events, OutgoingMessages) Post(
        CategoriseIncident command, 
        IncidentDetails existing, 
        User user)
    {
        var events = new Events();
        var messages = new OutgoingMessages();
        
        if (existing.Category != command.Category)
        {
            events += new IncidentCategorised
            {
                Category = command.Category,
                UserId = user.Id
            };

            // Send a command message to try to assign the priority
            messages.Add(new TryAssignPriority
            {
                IncidentId = existing.Id
            });
        }

        return (events, messages);
    }
}

public static class CategoriseIncidentHandler
{
    public static readonly Guid SystemId = Guid.NewGuid();
    
    [AggregateHandler]
    // The object? as return value will be interpreted
    // by Wolverine as appending one or zero events
    public static async Task<object?> Handle(
        CategoriseIncident command, 
        IncidentDetails existing,
        IMessageBus bus)
    {
        if (existing.Category != command.Category)
        {
            // Send the message to any and all subscribers to this message
            await bus.PublishAsync(new TryAssignPriority { IncidentId = existing.Id });
            return new IncidentCategorised
            {
                Category = command.Category,
                UserId = SystemId
            };
        }

        // Wolverine will interpret this as "do no work"
        return null;
    }
}