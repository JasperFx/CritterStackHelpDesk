using Helpdesk.Api;
using JasperFx.CodeGeneration;
using JasperFx.Core;
using Marten;
using Marten.Events.Projections;
using Marten.Exceptions;
using Npgsql;
using Oakton;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.FluentValidation;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;
using Wolverine.Marten;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

// Adds in some diagnostics
builder.Host.ApplyOaktonExtensions();

builder.Services.AddMarten(opts =>
{
    var connectionString = builder.Configuration.GetConnectionString("marten");
    opts.Connection(connectionString);
    
    opts.Projections.Add<IncidentDetailsProjection>(ProjectionLifecycle.Inline);
})
    // Adds Wolverine transactional middleware for Marten
    // and the Wolverine transactional outbox support as well
    .IntegrateWithWolverine()
    
    .EventForwardingToWolverine(opts =>
    {
        // Setting up a little transformation of an event with event metadata to an internal command message
        opts.SubscribeToEvent<IncidentCategorised>().TransformedTo(e => new TryAssignPriority
        {
            IncidentId = e.StreamId,
            UserId = e.Data.UserId
        });
    });

builder.Host.UseWolverine(opts =>
{
    opts.CodeGeneration.TypeLoadMode = TypeLoadMode.Static;
    
    // Let's build in some durability for transient errors
    opts.OnException<NpgsqlException>().Or<MartenCommandException>()
        .RetryWithCooldown(50.Milliseconds(), 100.Milliseconds(), 250.Milliseconds());
    
    // Apply the validation middleware *and* discover and register
    // Fluent Validation validators
    opts.UseFluentValidation();
    
    // Automatic transactional middleware
    opts.Policies.AutoApplyTransactions();
    
    // Opt into the transactional inbox for local 
    // queues
    opts.Policies.UseDurableLocalQueues();
    
    // Opt into the transactional inbox/outbox on all messaging
    // endpoints
    opts.Policies.UseDurableOutboxOnAllSendingEndpoints();
    
    // Connecting to a local Rabbit MQ broker
    // at the default port
    opts.UseRabbitMq();

    // Adding a single Rabbit MQ messaging rule
    opts.PublishMessage<RingAllTheAlarms>()
        .ToRabbitExchange("notifications");
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

app.MapWolverineEndpoints(opts =>
{
    opts.UseFluentValidationProblemDetailMiddleware();
    
    // Creates a User object in HTTP requests based on
    // the "user-id" claim
    opts.AddMiddleware(typeof(UserDetectionMiddleware));
});

// This is important for Wolverine/Marten diagnostics 
// and environment management
return await app.RunOaktonCommands(args);
