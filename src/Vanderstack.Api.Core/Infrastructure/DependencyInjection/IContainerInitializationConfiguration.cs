namespace Vanderstack.Api.Core.Infrastructure.DependencyInjection
{
    public interface IContainerInitializationConfiguration
    {
        SimpleInjector.Lifestyle DefaultLifestyle { get; }
        SimpleInjector.ScopedLifestyle DefaultScopedLifestyle { get; }
        bool RequireVerification { get; }
        IServicePackage ServicePackage { get; }
    }

    public class IContainerInitializationConfigurationPackage : IStartupServicePackage
    {
        public void RegisterStartupService(Container container)
        {
            container.RegisterConditional(
                serviceType: typeof(IContainerInitializationConfiguration)
                , registration: SimpleInjector.Lifestyle.Transient.CreateRegistration(
                    () => container.GetInstance<ContainerInitializationConfiguration>()
                    , container
                )
                , predicate: configuration => !configuration.Handled
            );
        }
    }
}
