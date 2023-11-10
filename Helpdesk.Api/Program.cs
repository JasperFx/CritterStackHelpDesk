using Helpdesk.Api;
using Marten;
using Marten.Events.Projections;
using Oakton;
using Wolverine;
using Wolverine.FluentValidation;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;
using Wolverine.Marten;

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
    
    .EventForwardingToWolverine();

builder.Host.UseWolverine(opts =>
{
    // Apply the validation middleware *and* discover and register
    // Fluent Validation validators
    opts.UseFluentValidation();
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

app.MapWolverineEndpoints(opts =>
{
    opts.UseFluentValidationProblemDetailMiddleware();
    opts.AddMiddleware(typeof(UserDetectionMiddlware));
});

app.MapWolverineEndpoints(opts =>
{
    opts.UseFluentValidationProblemDetailMiddleware();
    opts.AddMiddleware(typeof(UserDetectionMiddlware));
});

// This is important for Wolverine/Marten diagnostics 
// and environment management
return await app.RunOaktonCommands(args);
