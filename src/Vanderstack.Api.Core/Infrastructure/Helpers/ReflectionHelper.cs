using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Vanderstack.Api.Core.Infrastructure.Helpers
{
    public class ReflectionHelper
    {
        static ReflectionHelper()
        {
            Instance = new ReflectionHelper();
        }

        private IEnumerable<string> apiAssemblyNames = new Type[]
        {
            typeof(ReflectionHelper)
        }
        .Select(type =>
            type.GetTypeInfo().Assembly.GetName().Name.ToLower()
        );

        private ReflectionHelper()
        {
            Assemblies =
                DependencyContext.Default.RuntimeLibraries
                .Where(runtimeLibrary =>
                    apiAssemblyNames.Contains(runtimeLibrary.Name)
                    || runtimeLibrary.Dependencies.Any(dependency =>
                        apiAssemblyNames.Contains(dependency.Name.ToLower())
                    )
                )
                .Select(runtimeLibrary =>
                    new AssemblyName(runtimeLibrary.Name)
                ).Select(assemblyName =>
                    Assembly.Load(assemblyName)
                )
                .ToList()
                .AsReadOnly();

            Types =
                Assemblies.SelectMany(assembly =>
                    assembly.GetTypes()
                )
                .ToList()
                .AsReadOnly();
        }

        public static readonly ReflectionHelper Instance;

        public readonly IReadOnlyCollection<Assembly> Assemblies;
        public readonly IReadOnlyCollection<Type> Types;
    }
}
