using System.Linq;
using System.Security.Claims;
using JasperFx.Core;
using Marten.Events;
using Shouldly;
using Xunit;

namespace Helpdesk.Api.Tests;

public class log_incident : IntegrationContext
{
    public log_incident(AppFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task create_a_new_incident_happy_path()
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

        using var session = Store.LightweightSession();
        var events = await session.Events.FetchStreamAsync(incidentId);
        var logged = events.First().ShouldBeOfType<IncidentLogged>();

        // This deserves more assertions, but you get the point...
        logged.CustomerId.ShouldBe(BaselineData.Customer1Id);
    }

    [Fact]
    public async Task log_incident_with_invalid_customer()
    {
        // We'll need a user
        var user = new User(Guid.NewGuid());
        
        // Reject the new incident because the Customer for 
        // the command cannot be found
        var initial = await Scenario(x =>
        {
            var contact = new Contact(ContactChannel.Email);
            var nonExistentCustomerId = Guid.NewGuid();
            x.Post.Json(new LogIncident(nonExistentCustomerId, contact, "It's broken")).ToUrl("/api/incidents");
            x.StatusCodeShouldBe(400);
            
            x.WithClaim(new Claim("user-id", user.Id.ToString()));
        });
    }
}

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