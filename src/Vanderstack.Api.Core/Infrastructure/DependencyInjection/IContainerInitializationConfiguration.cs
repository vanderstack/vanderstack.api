using System;

namespace Vanderstack.Api.Core.Infrastructure.DependencyInjection
{
    public interface IContainerInitializationConfiguration : IStartupService
    {
        SimpleInjector.Lifestyle DefaultLifestyle { get; }
        SimpleInjector.ScopedLifestyle DefaultScopedLifestyle { get; }
        bool RequireVerification { get; }
        IObjectGraphConfiguration ObjectGraphConfiguration { get; }
    }

    public interface IService
    {

    }

    public interface ISystemService : IService
    {

    }

    public interface IStartupService : IService
    {

    }

    public interface IRuntimeService : IService
    {

    }

    public interface IObjectGraphConfiguration
    {
        void ConfigureObjectGraph(Container container);
        SimpleInjector.Lifestyle Lifestyle { get; }
    }

    public interface IObjectGraphConfiguration<out TService>
        : IObjectGraphConfiguration
        where TService : IService
    {

    }

    public interface IRuntimeServiceObjectGraphConfiguration<out TRuntimeService>
        : IObjectGraphConfiguration<TRuntimeService>
        where TRuntimeService : IRuntimeService
    {

    }

    public interface IStartupServiceObjectGraphConfiguration<out TStartupService>
        : IObjectGraphConfiguration<TStartupService>
        where TStartupService : IStartupService
    {

    }

    // This will initialize the lowest level container which is responsible
    // for returning configuration values related to the application environment
    // and other compile time known features.
    // this will include IEnumerable<Assembly>, IEnumerable<Type>, strongly typed Configuration, etc
    public interface ISystemServiceObjectGraphConfiguration<out TSystemService>
        : IObjectGraphConfiguration<TSystemService>
        where TSystemService : ISystemService
    {

    }

    public interface IStartupServiceDefaultGraphConfiguration<out TStartupService, out TStartupServiceImplementation>
        : IStartupServiceObjectGraphConfiguration<TStartupService>
        where TStartupService : IStartupService
        where TStartupServiceImplementation : class, TStartupService
    {
        Func<TStartupServiceImplementation> instanceCreator { get; }
    }

    public class IContainerInitializationConfigurationPackage
        : IStartupServiceDefaultGraphConfiguration<IContainerInitializationConfiguration, ContainerInitializationConfiguration>
    {
        // work in progress, make this use the lifestyle, factory, and predicate from the interface
        public void ConfigureObjectGraph(Container container)
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

    public class StartupServiceDefaultGraphConfiguration<TStartupService, TStartupServiceImplementation>
        : IStartupServiceDefaultGraphConfiguration<TStartupService, TStartupServiceImplementation>
        where TStartupService : IStartupService
        where TStartupServiceImplementation : class, TStartupService
    {
        public void ConfigureObjectGraph(Container container)
        {
            container.RegisterConditional(
                serviceType: typeof(TStartupService)
                , registration: SimpleInjector.Lifestyle.Transient.CreateRegistration(
                    () => container.GetInstance<TStartupServiceImplementation>()
                    , container
                )
                , predicate: configuration => !configuration.Handled
            );
        }
    }
}
