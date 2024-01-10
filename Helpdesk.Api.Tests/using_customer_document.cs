using System.Collections.Generic;
using System.Linq;
using Alba;
using FluentAssertions.Extensions;
using Marten;
using Shouldly;
using Xunit;

namespace Helpdesk.Api.Tests;

public class using_customer_document : IntegrationContext
{
    public using_customer_document(AppFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task persist_and_load_customer_data()
    {
        var customer = new Customer
        {
            Duration = new ContractDuration(new DateOnly(2023, 12, 1), new DateOnly(2024, 12, 1)),
            Region = "West Coast",
            Priorities = new Dictionary<IncidentCategory, IncidentPriority>
            {
                { IncidentCategory.Database, IncidentPriority.High }
            }
        };
        
        // As a convenience just because you'll use it so often in tests,
        // I made a property named "Store" on the base class for quick access to
        // the DocumentStore for the application
        // ALWAYS remember to dispose any sessions you open in tests!
        await using var session = Store.LightweightSession();
        
        // Tell Marten to save the new document
        session.Store(customer);

        // commit any pending changes
        await session.SaveChangesAsync();

        // Marten is assigning an Id for you when one doesn't already
        // exist, so that's where that value comes from
        var copy = await session.LoadAsync<Customer>(customer.Id);
        
        // Just proving to you that it's not the same object
        copy.ShouldNotBeSameAs(customer);
        
        copy.Duration.ShouldBe(customer.Duration);



        var results = await session.Query<Customer>()
            .Where(x => x.Region == "West Coast")
            .OrderByDescending(x => x.Duration.End)
            .ToListAsync();
    }
}