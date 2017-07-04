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
        public AssemblyProvider()
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

    public class RuntimeLibraryAssemblyNameProvider
    {
        public IEnumerable<string> AssemblyNames =>
            DependencyContext.Default.RuntimeLibraries
            .SelectMany(runtimeLibrary =>
                runtimeLibrary.Dependencies.SelectMany(dependency =>
                    dependency.Name
                ).Concat(
                    runtimeLibrary.Assemblies.Select(assembly =>
                        assembly.Name
                    )
                )
            );
                
    }

    public class VanderstackAssemblyNameProvider
    {
        public IEnumerable<string> AssemblyNames =>
            typeof(VanderstackAssemblyNameProvider).GetTypeInfo()
            .Assembly.GetName().Name.AsEnumerable();
    }

    public class UserDefinedAssemblyNameProvider
    {
        public UserDefinedAssemblyNameProvider(VanderstackAssemblyNameProvider vanderstackAssemblyNameProvider)
        {
            _vanderstackAssemblyNames = vanderstackAssemblyNameProvider.AssemblyNames;
        }

        private readonly IEnumerable<string> _vanderstackAssemblyNames;

        public IEnumerable<string> AssemblyNames =>
            DependencyContext.Default.RuntimeLibraries
            .Where(runtimeLibrary =>
                runtimeLibrary.Dependencies.Select(dependency =>
                    dependency.Name
                ).Intersect(_vanderstackAssemblyNames).Any()
            ).SelectMany(runtimeLibrary =>
                runtimeLibrary.Dependencies.Select(dependencyLibrary =>
                    dependencyLibrary.Name
                ).Concat(runtimeLibrary.Name.AsEnumerable())
            );
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
