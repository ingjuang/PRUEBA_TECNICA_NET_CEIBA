using EventosVivosBackNet.Infrastructure.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace EventosVivosBackNet.IntegrationTests.Fixtures
{
    public class EventosVivosWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .Build();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(_postgres.GetConnectionString()));
            });
        }

        public async Task InitializeAsync()
        {
            await _postgres.StartAsync();

            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await db.Database.ExecuteSqlRawAsync(CreateSchemaSQL());
            await SeedDataAsync(db);
        }

        private static string CreateSchemaSQL() => """
            CREATE TABLE IF NOT EXISTS venues (
                id BIGSERIAL PRIMARY KEY,
                name VARCHAR(100) NOT NULL UNIQUE,
                capacity INTEGER NOT NULL,
                city VARCHAR(100) NOT NULL
            );

            CREATE TABLE IF NOT EXISTS events (
                id BIGSERIAL PRIMARY KEY,
                title VARCHAR(100) NOT NULL,
                description VARCHAR(500) NOT NULL,
                venue_id BIGINT NOT NULL REFERENCES venues(id),
                max_capacity INTEGER NOT NULL,
                start_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
                end_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
                price NUMERIC(12,2) NOT NULL,
                event_type VARCHAR(20) NOT NULL,
                status VARCHAR(20) NOT NULL,
                created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS reservations (
                id BIGSERIAL PRIMARY KEY,
                event_id BIGINT NOT NULL REFERENCES events(id),
                quantity INTEGER NOT NULL,
                buyer_name VARCHAR(150) NOT NULL,
                buyer_email VARCHAR(254) NOT NULL,
                status VARCHAR(20) NOT NULL,
                reservation_code VARCHAR(20) UNIQUE,
                cancelled_at TIMESTAMP WITHOUT TIME ZONE,
                created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS reservation_losses (
                id BIGSERIAL PRIMARY KEY,
                reservation_id BIGINT NOT NULL UNIQUE REFERENCES reservations(id),
                event_id BIGINT NOT NULL REFERENCES events(id),
                quantity INTEGER NOT NULL,
                reason VARCHAR(50) NOT NULL,
                created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW()
            );

            CREATE INDEX IF NOT EXISTS idx_events_status_type ON events(status, event_type);
            CREATE INDEX IF NOT EXISTS idx_events_title ON events(title);
            CREATE INDEX IF NOT EXISTS idx_events_venue_dates ON events(venue_id, start_at, end_at);
            CREATE INDEX IF NOT EXISTS idx_reservations_email ON reservations(buyer_email);
            CREATE INDEX IF NOT EXISTS idx_reservations_event_status ON reservations(event_id, status);
            """;

        private static async Task SeedDataAsync(AppDbContext db)
        {
            await db.Database.ExecuteSqlRawAsync("""
                INSERT INTO venues (id, name, capacity, city) VALUES
                    (1, 'Auditorio Central', 200, 'Bogotá'),
                    (2, 'Sala Norte', 50, 'Bogotá'),
                    (3, 'Arena Sur', 500, 'Medellín')
                ON CONFLICT (id) DO NOTHING;
                SELECT setval(pg_get_serial_sequence('venues', 'id'), (SELECT COALESCE(MAX(id), 0) FROM venues));
                """);
        }

        public new async Task DisposeAsync()
        {
            await _postgres.DisposeAsync();
            await base.DisposeAsync();
        }
    }
}
