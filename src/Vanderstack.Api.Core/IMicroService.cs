using System;
using System.Linq;
using System.Reflection;
using Vanderstack.Api.Core.Infrastructure.DependencyInjection;
using Vanderstack.Api.Core.Infrastructure.Helpers;

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

    public class IMicroServiceServicePackage : IServiceGraphConfiguration
    {
        public void RegisterService(Container container)
        {
            var registrations =
                ReflectionHelper
                .Instance
                .Types
                .Where(candidateType =>
                    typeof(IMicroService).IsAssignableFrom(candidateType)
                    && candidateType.GetTypeInfo().IsClass
                ).Select(microserviceType =>
                    SimpleInjector.Lifestyle.Singleton.CreateRegistration(microserviceType, container)
                );

            container.RegisterCollection<IMicroService>(registrations);
        }
    }
}
