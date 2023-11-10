namespace Helpdesk.Api;

public class Customer
{
    public Guid Id { get; set; }

    public Dictionary<IncidentCategory, IncidentPriority> Priorities { get; set; }
        = new();
}