using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using Vanderstack.Api.Core.Infrastructure.DependencyInjection;
using Vanderstack.Api.Core.Infrastructure.Extensions;

namespace Vanderstack.Api.Core.Infrastructure.Providers
{
    public class AssemblyProvider
    {
        public AssemblyProvider(IAssemblyConfiguration assemblyConfiguration)
        {
            _assemblyConfiguration = assemblyConfiguration;
        }

        private readonly IAssemblyConfiguration _assemblyConfiguration;

        public IEnumerable<Assembly> Assemblies()
        {
            return DependencyContext.Default.RuntimeLibraries
                .Where(runtimeLibrary =>
                    IsMatchingAssembly(runtimeLibrary)
                    || HasDependencyMatchingAssembly(runtimeLibrary)
                ).Select(runtimeLibrary =>
                    new AssemblyName(runtimeLibrary.Name)
                ).Select(assemblyName =>
                    Assembly.Load(assemblyName)
                );
        }

        /// <summary>
        /// Determines whether the specified runtime library is a target Assembly.
        /// </summary>
        private bool IsMatchingAssembly(RuntimeLibrary runtimeLibrary)
        {
            return _assemblyConfiguration.AssemblyNamesToImport.Any(assemblyName =>
                string.Equals(assemblyName, runtimeLibrary.Name, System.StringComparison.OrdinalIgnoreCase)
            );
        }

        /// <summary>
        /// Determines whether the specified runtime library contains a target Assembly as a dependency.
        /// </summary>
        private bool HasDependencyMatchingAssembly(RuntimeLibrary runtimeLibrary)
        {
            return _assemblyConfiguration.AssemblyNamesToImport.Any(assemblyName =>
                runtimeLibrary.Dependencies.Any(dependency =>
                    string.Equals(assemblyName, dependency.Name, System.StringComparison.OrdinalIgnoreCase)
                )
            );
        }
    }

    public class CompositeAssemblyConfiguration : IAssemblyConfiguration, IComposite
    {
        public CompositeAssemblyConfiguration(IEnumerable<IAssemblyConfiguration> assemblyConfigurations)
        {
            _assemblyConfigurations = assemblyConfigurations;
        }

        private readonly IEnumerable<IAssemblyConfiguration> _assemblyConfigurations;

        public IEnumerable<string> AssemblyNamesToImport =>
            _assemblyConfigurations.SelectMany(assemblyConfiguration =>
                assemblyConfiguration.AssemblyNamesToImport
            );
    }

    public class VanderstackAssemblyConfiguration : IAssemblyConfiguration
    {
        // todo: Technical Debt
        // this will need to be extended with the addition of each new vanderstack extenstion project
        public IEnumerable<string> AssemblyNamesToImport =>
            typeof(VanderstackAssemblyConfiguration).GetTypeInfo()
            .Assembly.GetName().Name.AsEnumerable();
    }

    public class IAssemblyConfigurationStartupServicePackage : IStartupServicePackage
    {
        public void RegisterStartupService(Container container)
        {
            container.RegisterComposite<IAssemblyConfiguration>(SimpleInjector.Lifestyle.Transient);
        }
    }

    public interface IAssemblyConfiguration
    {
        IEnumerable<string> AssemblyNamesToImport { get; }
    }
}
