using System;
using System.Linq;
using System.Reflection;
using Vanderstack.Api.Core.Infrastructure.DependencyInjection;
using Vanderstack.Api.Core.Infrastructure.Internal;

namespace Vanderstack.Api.Core
{
    public class MockMicroService1 : IMicroService
    {
        public void Start()
        {
            throw new NotImplementedException();
        }
    }

    public class MockMicroService2 : IMicroService
    {
        public void Start()
        {
            throw new NotImplementedException();
        }
    }

    public interface IMicroService
    {
        void Start();
    }

    public class IMicroServiceServicePackage : IServicePackage
    {
        public void RegisterService(Container container)
        {
            var registrations =
                AssemblyProvider
                .Assemblies
                .SelectMany(assembly =>
                    assembly.ExportedTypes
                )
                .Where(candidateType =>
                    candidateType.GetTypeInfo().IsClass
                    && typeof(IMicroService).IsAssignableFrom(candidateType)
                ).Select(microserviceType =>
                    SimpleInjector.Lifestyle.Singleton.CreateRegistration(microserviceType, container)
                );

            container.RegisterCollection<IMicroService>(registrations);
        }
    }
}
