using System;

namespace Vanderstack.Api.Core.Infrastructure.DependencyInjection
{
    public class ContainerInitializationConfiguration : IContainerInitializationConfiguration
    {
        public ContainerInitializationConfiguration(
            SimpleInjector.Lifestyle defaultLifestyle
            , SimpleInjector.ScopedLifestyle defaultScopedLifestyle
            , IServiceGraphConfiguration servicePackage
            , bool requireVerification
        )
        {
            DefaultLifestyle = defaultLifestyle;
            DefaultScopedLifestyle = defaultScopedLifestyle;
            RequireVerification = requireVerification;
            ServiceConfiguration = servicePackage;
        }

        public SimpleInjector.Lifestyle DefaultLifestyle { get; }
        public SimpleInjector.ScopedLifestyle DefaultScopedLifestyle { get; }
        public bool RequireVerification { get; }
        public IServiceGraphConfiguration ServiceConfiguration { get; }
    }

    internal class ContainerInitializationConfigurationPackage : IStartupServiceObjectGraphConfiguration
    {
        public void RegisterStartupService(Container container)
        {
            Func<ContainerInitializationConfiguration> instanceCreator = () =>
            {
                var servicePackage = container.GetInstance<IServiceGraphConfiguration>();

                return new ContainerInitializationConfiguration(
                    defaultLifestyle: SimpleInjector.Lifestyle.Scoped
                    , defaultScopedLifestyle: new SimpleInjector.Lifestyles.AsyncScopedLifestyle()
                    , servicePackage: servicePackage
                    , requireVerification: true
                );
            };

            container.RegisterConditional(
                serviceType: typeof(ContainerInitializationConfiguration)
                , registration: SimpleInjector.Lifestyle.Transient.CreateRegistration(
                    instanceCreator
                    , container
                )
                , predicate: configuration => !configuration.Handled
            );
        }
    }
}
