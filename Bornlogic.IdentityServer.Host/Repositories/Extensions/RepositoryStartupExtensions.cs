namespace Bornlogic.IdentityServer.Host.Repositories.Extensions
{
    public static class RepositoryStartupExtensions
    {
        public static IServiceCollection RegisterStoreRepositories(this IServiceCollection services)
        {
            services.AddTransient<IApiResourceRepository, ApiResourceRepository>();
            services.AddTransient<IApiScopeRepository, ApiScopeRepository>();
            services.AddTransient<IClientRepository, ClientRepository>();
            services.AddTransient<IIdentityResourceRepository, IdentityResourceRepository>();
            services.AddTransient<IPersistentGrantRepository, PersistentGrantRepository>();
            services.AddTransient<IUserLoginsRepository, UserLoginsRepository>();
            services.AddTransient<IUserRepository, UsersRepository>();
            services.AddTransient<IUserTokensRepository, UserTokensRepository>();

            return services;
        }
    }
}
