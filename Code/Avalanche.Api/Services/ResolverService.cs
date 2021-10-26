
using Microsoft.Extensions.DependencyInjection;

namespace Avalanche.Api.Services
{
    public class ResolverService : IResolverService
    {
        private ServiceProvider _serviceProvider { get; }

        public ResolverService(IServiceCollection services) =>
            _serviceProvider = services.BuildServiceProvider();

        public T Resolve<T>() =>
            _serviceProvider.GetService<T>();
    }
}
