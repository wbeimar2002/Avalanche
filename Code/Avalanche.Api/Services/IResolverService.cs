namespace Avalanche.Api.Services
{
    public interface IResolverService
    {
        T Resolve<T>();
    }
}
