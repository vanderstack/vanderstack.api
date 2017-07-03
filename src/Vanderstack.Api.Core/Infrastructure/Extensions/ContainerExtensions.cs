using System.Linq;
using System.Reflection;
using Vanderstack.Api.Core.Infrastructure.DependencyInjection;
using Vanderstack.Api.Core.Infrastructure.Internal;

namespace Vanderstack.Api.Core.Infrastructure.Extensions
{
    public static class ContainerExtensions
    {
        public static void RegisterComposite<TService>(this Container container, SimpleInjector.Lifestyle lifestyle) where TService : class
        {
            var typesToRegister = AssemblyProvider.Assemblies.SelectMany(assembly =>
                assembly.ExportedTypes
            )
            .Where(candidateType =>
                candidateType.GetTypeInfo().IsClass
                && typeof(TService).IsAssignableFrom(candidateType)
            );

            var compositeType = typesToRegister.Where(candidateType =>
                typeof(IComposite).IsAssignableFrom(candidateType)
            ).Single();

            var implementationTypes = typesToRegister.Except(compositeType.AsEnumerable());
            var implementationRegistrations = implementationTypes.Select(implementationType =>
                lifestyle.CreateRegistration(implementationType, container)
            );

            container.RegisterConditional(
                serviceType: typeof(TService)
                , implementationType: compositeType
                , lifestyle: lifestyle
                , predicate: configuration => !configuration.Handled
            );

            container.RegisterCollection<TService>(implementationRegistrations);
        }
    }
}
