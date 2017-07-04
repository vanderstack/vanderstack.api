using System;
using SimpleInjector;

namespace Vanderstack.Api.Core.Infrastructure.DependencyInjection
{
    public class InitializedContainerProxy : IContainer
    {
        public InitializedContainerProxy(IContainer initializedContainer)
        {
            _initializedContainer = initializedContainer;
        }

        private readonly IContainer _initializedContainer;

        public TService GetInstance<TService>() where TService : class
            => _initializedContainer.GetInstance<TService>();
    }

    internal class InitializedContainerStartupServicePackage : IStartupServiceObjectGraphConfiguration
    {
        public void RegisterStartupService(Container container)
        {
            Func<InitializedContainerProxy> instanceCreator = () =>
            {
                var uninitializedContainer = container.GetInstance<Container>();
                var containerInitializer = container.GetInstance<IContainerInitializer>();
                var initializedContainer = containerInitializer.InitializeContainer(uninitializedContainer);
                return new InitializedContainerProxy(initializedContainer);
            };

            var registration = SimpleInjector.Lifestyle.Singleton.CreateRegistration(
                instanceCreator
                , container
            );

            container.RegisterConditional(
                serviceType: typeof(InitializedContainerProxy)
                , registration: registration
                , predicate: configuration => !configuration.Handled
            );
        }
    }
}
