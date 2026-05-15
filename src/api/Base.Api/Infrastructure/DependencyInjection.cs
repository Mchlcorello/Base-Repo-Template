using Base.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Base.Api.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("AppDb")));

        return services;
    }
}
