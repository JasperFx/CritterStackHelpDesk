using System.Security.Claims;
using FluentValidation;
using Marten;
using Marten.Schema;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;
using Wolverine.Marten;

namespace Helpdesk.Api;

public class CategoriseIncident
{
    [Identity]
    public Guid Id { get; set; }
    public IncidentCategory Category { get; set; }
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

public record User(Guid Id);

public static class UserDetectionMiddlware
{
    public static (User, ProblemDetails) Load(ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirst("user-id");
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var id))
        {
            return (new User(id), WolverineContinue.NoProblems);
        }
        
        return (new User(Guid.Empty), new ProblemDetails { Detail = "No valid user" });
    }
}

public static class CategoriseIncidentEndpoint
{
    [WolverinePost("/incidents/categorise"), AggregateHandler]
    public static IEnumerable<object> Post(CategoriseIncident command, IncidentDetails existing, User user)
    {
        if (existing.Category != command.Category)
        {
            yield return new IncidentCategorised
            {
                Category = command.Category,
                UserId = user.Id
            };
        }
    }
}


public static class CategoriseIncidentHandler
{
    public static readonly Guid SystemId = Guid.NewGuid();
    
    [AggregateHandler]
    public static IEnumerable<object> Handle(CategoriseIncident command, IncidentDetails existing)
    {
        if (existing.Category != command.Category)
        {
<<<<<<< HEAD
            yield return new IncidentCategorised
            {
                Category = command.Category,
                UserId = SystemId
            };
=======
            yield return new IncidentCategorised(command.Category, SystemId);
>>>>>>> 759f974 (Wolverine HTTP endpoints)
        }
    }
}