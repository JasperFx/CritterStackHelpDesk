using System.Linq;
using Shouldly;
using Xunit;

namespace Helpdesk.Api.Tests;

public class CategoriseIncidentTests
{
    [Fact]
    public void raise_categorized_event_if_changed()
    {
        var command = new CategoriseIncident
        {
            Category = IncidentCategory.Database
        };

        var details = new IncidentDetails(Guid.NewGuid(), Guid.NewGuid(), IncidentStatus.Closed, new IncidentNote[0],
            IncidentCategory.Hardware);

        var user = new User(Guid.NewGuid());
        var events = CategoriseIncidentEndpoint.Post(command, details, user);

        var categorised = events.Single().ShouldBeOfType<IncidentCategorised>();
        categorised
            .Category.ShouldBe(IncidentCategory.Database);
        
        categorised.UserId.ShouldBe(user.Id);
    }
}