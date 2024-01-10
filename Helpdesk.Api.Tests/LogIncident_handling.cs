using System.Linq;
using Shouldly;
using Xunit;

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