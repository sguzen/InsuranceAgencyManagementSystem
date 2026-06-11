using IAMS.Domain.Entities;
using IAMS.Persistence.Contexts;
using IAMS.MultiTenancy.Data;
using IAMS.MultiTenancy.Entities;

namespace IAMS.IntegrationTests.Fixtures;

public static class DatabaseSeeder
{
    public static void SeedTestData(ApplicationDbContext appDb, TenantDbContext masterDb)
    {
        // Seed master database with test tenants
        if (!masterDb.Tenants.Any(t => t.Identifier == "test-agency-1"))
        {
            var tenants = new[]
            {
                new TenantEntity
                {
                    Id = 2,
                    Name = "Test Agency 1",
                    Identifier = "test-agency-1",
                    ConnectionString = "InMemory",
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow,
                    SubscriptionPlan = "Standard",
                    SubscriptionExpiry = DateTime.UtcNow.AddYears(1),
                    MaxUsers = 50,
                    ContactEmail = "admin@testagency1.com",
                    TimeZone = "Europe/Istanbul",
                    Currency = "TRY",
                    Language = "tr"
                },
                new TenantEntity
                {
                    Id = 3,
                    Name = "Test Agency 2",
                    Identifier = "test-agency-2",
                    ConnectionString = "InMemory",
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow,
                    SubscriptionPlan = "Premium",
                    SubscriptionExpiry = DateTime.UtcNow.AddYears(1),
                    MaxUsers = 100,
                    ContactEmail = "admin@testagency2.com",
                    TimeZone = "Europe/Istanbul",
                    Currency = "TRY",
                    Language = "tr"
                }
            };

            masterDb.Tenants.AddRange(tenants);
            masterDb.SaveChanges();
        }

        // Seed application database with test customers
        if (!appDb.Customers.Any())
        {
            var customers = new[]
            {
                new Customer
                {
                    Id = 1,
                    FirstName = "Ahmet",
                    LastName = "Özkan",
                    Email = "ahmet.ozkan@example.com",
                    Phone = "+90 533 123 4567",
                    CreatedOn = DateTime.UtcNow
                },
                new Customer
                {
                    Id = 2,
                    FirstName = "Fatma",
                    LastName = "Demir",
                    Email = "fatma.demir@example.com",
                    Phone = "+90 533 765 4321",
                    CreatedOn = DateTime.UtcNow
                }
            };

            appDb.Customers.AddRange(customers);
            appDb.SaveChanges();
        }
    }
}