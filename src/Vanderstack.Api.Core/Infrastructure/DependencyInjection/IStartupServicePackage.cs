namespace Vanderstack.Api.Core.Infrastructure.DependencyInjection
{
    public interface IStartupServicePackage
    {
        void RegisterStartupService(Container container);
    }
}
