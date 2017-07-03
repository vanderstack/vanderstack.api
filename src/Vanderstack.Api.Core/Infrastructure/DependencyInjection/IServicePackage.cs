using Vanderstack.Api.Core.Infrastructure.Extensions;

namespace Vanderstack.Api.Core.Infrastructure.DependencyInjection
{
    public interface IServicePackage
    {
        void RegisterService(Container container);
    }

    internal class IServicePackageStartupServicePackage : IStartupServicePackage
    {
        public void RegisterStartupService(Container container)
        {
            container.RegisterComposite<IServicePackage>(SimpleInjector.Lifestyle.Transient);
        }
    }
}
