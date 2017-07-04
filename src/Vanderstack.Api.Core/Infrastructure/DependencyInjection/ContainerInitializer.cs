using SimpleInjector;

namespace Vanderstack.Api.Core.Infrastructure.DependencyInjection
{
    public class ContainerInitializer : IContainerInitializer
    {
        public ContainerInitializer(IContainerInitializationConfiguration containerConfiguration)
        {
            _containerConfiguration = containerConfiguration;
        }

        private readonly IContainerInitializationConfiguration _containerConfiguration;

        public IContainer InitializeContainer(Container targetContainer)
        {
            targetContainer.Options.DefaultLifestyle = _containerConfiguration.DefaultLifestyle;
            targetContainer.Options.DefaultScopedLifestyle = _containerConfiguration.DefaultScopedLifestyle;

            _containerConfiguration.ObjectGraphConfiguration.RegisterService(targetContainer);

            if (_containerConfiguration.RequireVerification)
            {
                targetContainer.Verify();
            }

            return targetContainer;
        }
    }

    internal class ContainerInitializerStartupServicePackage : IStartupServiceObjectGraphConfiguration
    {
        public void RegisterStartupService(Container container)
        {
            container.RegisterConditional<ContainerInitializer, ContainerInitializer>(
                lifestyle: SimpleInjector.Lifestyle.Transient
                , predicate: configuration => !configuration.Handled
            );
        }
    }
}
