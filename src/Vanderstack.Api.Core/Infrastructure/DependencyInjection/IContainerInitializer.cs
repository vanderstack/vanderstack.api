namespace Vanderstack.Api.Core.Infrastructure.DependencyInjection
{
    public interface IContainerInitializer
    {
        IContainer InitializeContainer(Container targetContainer);
    }

    internal class IContainerInitializerStartupServicePackage : IStartupServiceObjectGraphConfiguration
    {
        public void RegisterStartupService(Container container)
        {
            container.RegisterConditional(
                serviceType: typeof(IContainerInitializer)
                , registration: SimpleInjector.Lifestyle.Transient.CreateRegistration(
                    () => container.GetInstance<ContainerInitializer>()
                    , container
                )
                , predicate: configuration => !configuration.Handled
            );
        }
    }
}
