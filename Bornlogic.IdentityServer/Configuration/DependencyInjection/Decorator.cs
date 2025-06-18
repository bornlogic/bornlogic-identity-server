namespace Bornlogic.IdentityServer.Configuration.DependencyInjection
{
    internal class Decorator<TService>
    {
        public TService Instance { get; set; }

        public Decorator(TService instance)
        {
            Instance = instance;
        }
    }

    internal class Decorator<TService, TImpl> : Decorator<TService>
        where TImpl : class, TService
    {
        public Decorator(TImpl instance) : base(instance)
        {
        }
    }

    internal class DisposableDecorator<TService> : Decorator<TService>, IDisposable
    {
        public DisposableDecorator(TService instance) : base(instance)
        {
        }

        public void Dispose()
        {
            (Instance as IDisposable)?.Dispose();
        }
    }
}
