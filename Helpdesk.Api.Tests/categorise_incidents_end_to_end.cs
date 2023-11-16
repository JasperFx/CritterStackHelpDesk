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
    public async Task categorise_an_incident_happy_path()
    {
        // We'll need a user
        var user = new User(Guid.NewGuid());
        
        

        // Log a new incident first
        var initial = await Scenario(x =>
        {
            var contact = new Contact(ContactChannel.Email);
            x.Post.Json(new LogIncident(BaselineData.Customer1Id, contact, "It's broken")).ToUrl("/api/incidents");
            x.StatusCodeShouldBe(201);
            
            x.WithClaim(new Claim("user-id", user.Id.ToString()));
        });

        var incidentId = initial.ReadAsJson<NewIncidentResponse>().IncidentId;

        // POST a CategoriseIncident to the HTTP endpoint, wait until all
        // cascading Wolverine processing is complete
        var (session, result) = await TrackedHttpCall(x =>
        {
            x.Post.Json(new CategoriseIncident
            {
                Category = IncidentCategory.Database,
                Id = incidentId,
                Version = 1
            }).ToUrl("/api/incidents/categorise");

            x.StatusCodeShouldBe(204);
            
            x.WithClaim(new Claim("user-id", user.Id.ToString()));
        });

        // Assert that the event forwarded as a command message
        session.Executed.SingleMessage<TryAssignPriority>()
            .ShouldNotBeNull();
    }
    
}