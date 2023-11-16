using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json.Serialization;
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

public record User(Guid Id);

public static class UserDetectionMiddleware
{
    public static (User, ProblemDetails) Load(ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirst("user-id");
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var id))
        {
            return (new User(id), WolverineContinue.NoProblems);
        }
        
        return (new User(Guid.Empty), new ProblemDetails { Detail = "No valid user", Status = 400});
    }
}

public static class CategoriseIncidentEndpoint
{
    [WolverinePost("/api/incidents/categorise"), AggregateHandler]
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
            yield return new IncidentCategorised
            {
                Category = command.Category,
                UserId = SystemId
            };
        }
    }
}