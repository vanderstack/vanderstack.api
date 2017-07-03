using System;
using System.Linq;
using System.Reflection;
using Vanderstack.Api.Core.Infrastructure.DependencyInjection;

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
                AssemblyProvider
                .Assemblies
                .SelectMany(assembly =>
                    assembly.ExportedTypes
                )
                .Where(candidateType =>
                    candidateType.GetTypeInfo().IsClass
                    && typeof(IStartupServicePackage).IsAssignableFrom(candidateType)
                )
                .Select(startupPackageType =>
                    (IStartupServicePackage)Activator.CreateInstance(startupPackageType)
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
