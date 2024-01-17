using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace Helpdesk.Api;

// Using the custom type makes it easier
// for the Wolverine code generation to route
// things around. I'm not ashamed.
public record User(Guid Id);

public static class UserDetectionMiddleware
{
    public static (User, ProblemDetails) Load(ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirst("user-id");
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var id))
        {
            // Everything is good, keep on trucking with this request!
            return (new User(id), WolverineContinue.NoProblems);
        }
        
        // Nope, nope, nope. We got problems, so stop the presses and emit a ProblemDetails response
        // with a 400 status code telling the caller that there's no valid user for this request
        return (new User(Guid.Empty), new ProblemDetails { Detail = "No valid user", Status = 400});
    }
}