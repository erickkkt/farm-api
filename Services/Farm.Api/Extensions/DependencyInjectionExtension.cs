using Farm.Business.Services;
using Farm.Business.Services.Interfaces;
using Farm.Domain.Repositories;
using Farm.Domain.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Farm.Api.Extensions
{
    /// <summary>
    /// Dependency Injection helper
    /// </summary>
    public static class DependencyInjectionExtension
    {
        /// <summary>
        /// AddBaseSettings method
        /// </summary>
        public static void AddBaseSettings(this IServiceCollection services, IConfigurationRoot configurationRoot)
        {
            // Base API Configuration Access
            
        }

        /// <summary>
        /// AddRepositories method
        /// </summary>
        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IAnimalRepository, AnimalRepository>();
            services.AddScoped<IFarmRepository, FarmRepository>();
            services.AddScoped<ICageRepository, CageRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
        }


        /// <summary>
        /// AddServices method
        /// </summary>
        public static void AddServices(this IServiceCollection services, IConfigurationRoot configurationRoot)
        {
            services.AddSingleton<IMemoryCache, MemoryCache>();            

            services.AddScoped<IAnimalService, AnimalService>();
            services.AddScoped<IFarmService, FarmService>();
            services.AddScoped<ICageService, CageService>();
            services.AddScoped<IUserService, UserService>();
        }

        public static void AddSecurityHeaders(this IApplicationBuilder app)
        {
            app.UseXfo(options => options.SameOrigin());
            app.UseHsts(options => options.MaxAge(days: 365).IncludeSubdomains());
            app.UseXXssProtection(options => options.EnabledWithBlockMode());
            app.UseXContentTypeOptions();
            app.UseCsp(options => options
                .DefaultSources(s => s.Self().CustomSources("data:").CustomSources("https:"))
                .StyleSources(s => s.CustomSources("*").UnsafeInline())
                .ScriptSources(s => s.Self().CustomSources("*").UnsafeInline())
                .FontSources(s => s.CustomSources("*"))
                .ImageSources(s => s.CustomSources("*"))
            );
            app.UseReferrerPolicy(opts => opts.StrictOrigin());
        }
    }
}
