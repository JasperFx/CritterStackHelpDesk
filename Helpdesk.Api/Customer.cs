namespace Helpdesk.Api;

public class Customer
{
    public Guid Id { get; set; }

    // We'll use this later for some "logic" about how incidents
    // can be automatically prioritized
    public Dictionary<IncidentCategory, IncidentPriority> Priorities { get; set; }
        = new();
    
    public string? Region { get; set; }
    
    public ContractDuration Duration { get; set; } 
}

public record ContractDuration(DateOnly Start, DateOnly End);
