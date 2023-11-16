using FluentValidation;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Attributes;
using Wolverine.Http;
using Wolverine.Marten;

namespace Helpdesk.Api;

public record LogIncident(
    Guid CustomerId,
    Contact Contact,
    string Description
)
{
    public class LogIncidentValidator : AbstractValidator<LogIncident>
    {
        public LogIncidentValidator()
        {
            RuleFor(x => x.Description).NotEmpty().NotNull();
            RuleFor(x => x.Contact).NotNull();
        }
    }
};

public record NewIncidentResponse(Guid IncidentId) 
    : CreationResponse("/api/incidents/" + IncidentId);

public static class LogIncidentEndpoint
{
    [WolverineBefore]
    public static async Task<ProblemDetails> ValidateCustomer(LogIncident command, IDocumentSession session)
    {
        var exists = await session.Query<Customer>().AnyAsync(x => x.Id == command.CustomerId);
        return exists
            ? WolverineContinue.NoProblems
            : new ProblemDetails { Detail = $"Unknown customer id {command.CustomerId}", Status = 400};
    }
    
    [WolverinePost("/api/incidents")]
    public static (NewIncidentResponse, IStartStream) Post(LogIncident command, User user)
    {
        var logged = new IncidentLogged(command.CustomerId, command.Contact, command.Description, user.Id);

        var op = MartenOps.StartStream<Incident>(logged);

        return (new NewIncidentResponse(op.StreamId), op);
    }
}