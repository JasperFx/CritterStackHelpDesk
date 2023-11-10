using Helpdesk.Api;
using Marten;
using Marten.Events.Projections;
using Oakton;
using Wolverine;
using Wolverine.Marten;

var builder = WebApplication.CreateBuilder(args);

// Adds in some diagnostics
builder.Host.ApplyOaktonExtensions();

builder.Services.AddMarten(opts =>
{
    var connectionString = builder.Configuration.GetConnectionString("marten");
    opts.Connection(connectionString);
    
    opts.Projections.Add<IncidentDetailsProjection>(ProjectionLifecycle.Live);
})
    // Adds Wolverine transactional middleware for Marten
    // and the Wolverine transactional outbox support as well
    .IntegrateWithWolverine();

builder.Host.UseWolverine(opts =>
{
    opts.Policies.AutoApplyTransactions();
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// This is important for Wolverine/Marten diagnostics 
// and environment management
return await app.RunOaktonCommands(args);
