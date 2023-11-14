using System.Security.Claims;
using JasperFx.Core;
using Marten.Events;
using Shouldly;
using Xunit;

namespace Helpdesk.Api.Tests;

public class categorise_incidents_end_to_end : IntegrationContext
{
    public categorise_incidents_end_to_end(AppFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task categorise_an_incident()
    {
        var incidentId = CombGuidIdGeneration.NewGuid();

        // Log a new incident first
        await Scenario(x =>
        {
            var contact = new Contact(ContactChannel.Email);
            x.Post.Json(new LogIncident(Guid.NewGuid(), contact, "It's broken")).ToUrl("/api/incidents/categorise");
            x.StatusCodeShouldBe(201);
        });

        // We'll need a user
        var user = new User(Guid.NewGuid());


        var (session, result) = await TrackedHttpCall(x =>
        {
            x.Post.Json(new CategoriseIncident
            {
                Category = IncidentCategory.Database,
                Id = incidentId,
                Version = 1
            });

            x.StatusCodeShouldBe(204);
            
            x.WithClaim(new Claim("user-id", user.Id.ToString()));
        });

        // Assert that the event was executed as a message
        session.Executed.SingleMessage<IEvent<IncidentCategorised>>()
            .ShouldNotBeNull();
        
        session.Sent.SingleMessage<RingAllTheAlarms>()
            .IncidentId.ShouldBe(incidentId);

    }
}