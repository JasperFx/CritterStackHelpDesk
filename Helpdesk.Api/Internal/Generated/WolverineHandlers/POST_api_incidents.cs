// <auto-generated/>
#pragma warning disable
using FluentValidation;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;
using Wolverine.Marten.Publishing;
using Wolverine.Runtime;

namespace Internal.Generated.WolverineHandlers
{
    // START: POST_api_incidents
    public class POST_api_incidents : Wolverine.Http.HttpHandler
    {
        private readonly Wolverine.Http.WolverineHttpOptions _wolverineHttpOptions;
        private readonly FluentValidation.IValidator<Helpdesk.Api.LogIncident> _validator;
        private readonly Wolverine.Marten.Publishing.OutboxedSessionFactory _outboxedSessionFactory;
        private readonly Wolverine.Http.FluentValidation.IProblemDetailSource<Helpdesk.Api.LogIncident> _problemDetailSource;
        private readonly Wolverine.Runtime.IWolverineRuntime _wolverineRuntime;

        public POST_api_incidents(Wolverine.Http.WolverineHttpOptions wolverineHttpOptions, FluentValidation.IValidator<Helpdesk.Api.LogIncident> validator, Wolverine.Marten.Publishing.OutboxedSessionFactory outboxedSessionFactory, Wolverine.Http.FluentValidation.IProblemDetailSource<Helpdesk.Api.LogIncident> problemDetailSource, Wolverine.Runtime.IWolverineRuntime wolverineRuntime) : base(wolverineHttpOptions)
        {
            _wolverineHttpOptions = wolverineHttpOptions;
            _validator = validator;
            _outboxedSessionFactory = outboxedSessionFactory;
            _problemDetailSource = problemDetailSource;
            _wolverineRuntime = wolverineRuntime;
        }



        public override async System.Threading.Tasks.Task Handle(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            var messageContext = new Wolverine.Runtime.MessageContext(_wolverineRuntime);
            // Building the Marten session
            await using var documentSession = _outboxedSessionFactory.OpenSession(messageContext);
            // Reading the request body via JSON deserialization
            var (command, jsonContinue) = await ReadJsonAsync<Helpdesk.Api.LogIncident>(httpContext);
            if (jsonContinue == Wolverine.HandlerContinuation.Stop) return;
            
            // Execute FluentValidation validators
            var result1 = await Wolverine.Http.FluentValidation.Internals.FluentValidationHttpExecutor.ExecuteOne<Helpdesk.Api.LogIncident>(_validator, _problemDetailSource, command).ConfigureAwait(false);

            // Evaluate whether or not the execution should be stopped based on the IResult value
            if (!(result1 is Wolverine.Http.WolverineContinue))
            {
                await result1.ExecuteAsync(httpContext).ConfigureAwait(false);
                return;
            }


            (var user, var problemDetails2) = Helpdesk.Api.UserDetectionMiddleware.Load(httpContext.User);
            // Evaluate whether the processing should stop if there are any problems
            if (!(ReferenceEquals(problemDetails2, Wolverine.Http.WolverineContinue.NoProblems)))
            {
                await WriteProblems(problemDetails2, httpContext).ConfigureAwait(false);
                return;
            }


            var problemDetails3 = await Helpdesk.Api.LogIncidentEndpoint.ValidateCustomer(command, documentSession).ConfigureAwait(false);
            // Evaluate whether the processing should stop if there are any problems
            if (!(ReferenceEquals(problemDetails3, Wolverine.Http.WolverineContinue.NoProblems)))
            {
                await WriteProblems(problemDetails3, httpContext).ConfigureAwait(false);
                return;
            }


            
            // The actual HTTP request handler execution
            (var newIncidentResponse_response, var startStream) = Helpdesk.Api.LogIncidentEndpoint.Post(command, user);

            
            // Placed by Wolverine's ISideEffect policy
            startStream.Execute(documentSession);

            // This response type customizes the HTTP response
            ApplyHttpAware(newIncidentResponse_response, httpContext);
            
            // Commit any outstanding Marten changes
            await documentSession.SaveChangesAsync(httpContext.RequestAborted).ConfigureAwait(false);

            
            // Have to flush outgoing messages just in case Marten did nothing because of https://github.com/JasperFx/wolverine/issues/536
            await messageContext.FlushOutgoingMessagesAsync().ConfigureAwait(false);

            // Writing the response body to JSON because this was the first 'return variable' in the method signature
            await WriteJsonAsync(httpContext, newIncidentResponse_response);
        }

    }

    // END: POST_api_incidents
    
    
}

