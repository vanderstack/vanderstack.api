using Vanderstack.Api.Core.Infrastructure.Extensions;

namespace Vanderstack.Api.Core.Infrastructure.DependencyInjection
{
    public interface IServiceGraphConfiguration : IObjectGraphConfiguration
    {
        void RegisterService(Container container);
    }

    internal class IServicePackageStartupServicePackage : IStartupServiceObjectGraphConfiguration
    {
        public void RegisterStartupService(Container container)
        {
            container.RegisterComposite<IServiceGraphConfiguration>(SimpleInjector.Lifestyle.Transient);
        }
    }
}
