using System.Linq;
using System.Net.Http.Json;
using System.Security.Claims;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Helpdesk.Api.Tests;

public class LogIncident_handling
{
    [Fact]
    public void handle_the_log_incident_command()
    {
        // This is trivial, but the point is that 
        // we now have a pure function that can be
        // unit tested by pushing inputs in and measuring
        // outputs without any pesky mock object setup
        var contact = new Contact(ContactChannel.Email);
        var theCommand = new LogIncident(BaselineData.Customer1Id, contact, "It's broken");

        var theUser = new User(Guid.NewGuid());

        var (_, stream) = LogIncidentEndpoint.Post(theCommand, theUser);

        var logged = stream.Events.Single()
            .ShouldBeOfType<IncidentLogged>();
        
        logged.CustomerId.ShouldBe(theCommand.CustomerId);
        logged.Contact.ShouldBe(theCommand.Contact);
        logged.LoggedBy.ShouldBe(theUser.Id);
    }
}

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
        var logged = events.First().Data.ShouldBeOfType<IncidentLogged>();

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

public class log_incident_end_to_end : IntegrationContext
{
    private readonly ITestOutputHelper _output;

    public log_incident_end_to_end(AppFixture fixture, ITestOutputHelper output) : base(fixture)
    {
        _output = output;
    }

    [Fact]
    public async Task happy_path()
    {
        // We'll need a user
        var user = new User(Guid.NewGuid());
        
        // Log a new incident first
        var initial = await Scenario(x =>
        {
            var contact = new Contact(ContactChannel.Email);
            x.Post.Json(new LogIncident(BaselineData.Customer1Id, contact, "It's broken")).ToUrl("/api/incidents");
            x.StatusCodeShouldBe(201);
            
            // We need to set up a claim to refer to the user
            x.WithClaim(new Claim("user-id", user.Id.ToString()));
        });

        var incidentId = initial.ReadAsJson<NewIncidentResponse>().IncidentId;

        await using var session = Store.LightweightSession();
        var incident = await session.Events.AggregateStreamAsync<IncidentDetails>(incidentId);
        
        _output.WriteLine(JsonConvert.SerializeObject(incident));
    }
    
    
}