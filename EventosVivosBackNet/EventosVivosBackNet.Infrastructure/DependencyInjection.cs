using EventosVivosBackNet.Application.Commond.Interfaces.Repositories;
using EventosVivosBackNet.Application.Commond.Struct;
using EventosVivosBackNet.Infrastructure.Context;
using EventosVivosBackNet.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventosVivosBackNet.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(
                    Environment.GetEnvironmentVariable(ConfigurationStruct.DbConnectionString)));

            services.AddScoped<IUnitOfWorkDbEventosVivos, UnitOfWorkEventosVivos>();

            return services;
        }
    }
}
