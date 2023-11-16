using Helpdesk.Api;
using Marten;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Spectre.Console;

// From the docker compose file
var connectionString = "Host=localhost;Port=5433;Database=postgres;Username=postgres;password=postgres";
await using var store = DocumentStore.For(connectionString);

await using var session = store.LightweightSession();

var contact = new Contact(ContactChannel.Email, "Han", "Solo");
var userId = Guid.NewGuid();

var incidentId = session.Events.StartStream<Incident>(
    new IncidentLogged(Guid.NewGuid(), contact, "Software is crashing",userId),
    new IncidentCategorised
    {
        Category = IncidentCategory.Database,
        UserId = userId
    }
    
).Id;

await session.SaveChangesAsync();

session.Events.Append(incidentId, new IncidentPrioritised(IncidentPriority.High, userId));
await session.SaveChangesAsync();


// JSON junk
var settings = new JsonSerializerSettings
{
    Formatting = Formatting.Indented
};
settings.Converters.Add(new StringEnumConverter());


AnsiConsole.WriteLine();
AnsiConsole.WriteLine();
AnsiConsole.MarkupLine("[green]The persisted event data:[/]");

var events = await session.Events.FetchStreamAsync(incidentId);
foreach (var e in events)
{
    Console.WriteLine(JsonConvert.SerializeObject(e, settings));
}


AnsiConsole.WriteLine();
AnsiConsole.WriteLine();
AnsiConsole.MarkupLine("[green]The current state of the new incident:[/]");

// Skipping ahead here, but let's load the current state of the Incident
// by using a Marten "Live" aggregation
var incident = await session.Events.AggregateStreamAsync<Incident>(incidentId);


Console.WriteLine(JsonConvert.SerializeObject(incident, settings));
