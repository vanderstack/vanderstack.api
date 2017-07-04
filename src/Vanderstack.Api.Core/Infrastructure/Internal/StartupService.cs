using System;
using System.Linq;
using System.Reflection;
using Vanderstack.Api.Core.Infrastructure.DependencyInjection;
using Vanderstack.Api.Core.Infrastructure.Helpers;

namespace Vanderstack.Api.Core.Infrastructure.Internal
{
    internal class StartupService
    {
        internal StartupService() : this(new Container())
        {
        }

        internal StartupService(Container configurationContainer)
        {
            _configurationContainer = configurationContainer;

            ConfigureContainerOptions();
            ApplyContainerRegistrations();
            VerifyContainer();
        }

        internal IServiceRunner ServiceRunner =>
            _serviceContainer.GetInstance<IServiceRunner>();

        private readonly Container _configurationContainer;

        private IContainer _serviceContainer =>
            _configurationContainer.GetInstance<IContainer>();

        private void ConfigureContainerOptions()
        {
            _configurationContainer.Options.DefaultLifestyle = SimpleInjector.Lifestyle.Transient;
        }

        private void ApplyContainerRegistrations()
        {
            var startupServicePackages =
                ReflectionHelper
                .Instance
                .Types
                .Where(candidateType =>
                    typeof(IStartupServiceObjectGraphConfiguration).IsAssignableFrom(candidateType)
                    && candidateType.GetTypeInfo().IsClass
                ).Select(startupPackageType =>
                    (IStartupServiceObjectGraphConfiguration)Activator.CreateInstance(startupPackageType)
                );

            foreach (var startupServicePackage in startupServicePackages)
            {
                startupServicePackage.RegisterStartupService(_configurationContainer);
            }
        }

        private void VerifyContainer()
        {
            _configurationContainer.Verify();
        }
    }
}
