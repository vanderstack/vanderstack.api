using System;
using System.Collections.Generic;
using Vanderstack.Api.Core.Infrastructure.DependencyInjection;

namespace Vanderstack.Api.Core
{
    public class ServiceRunner : IServiceRunner
    {
        public ServiceRunner(IEnumerable<IMicroService> services)
        {
            _services = services;
        }

        private readonly IEnumerable<IMicroService> _services;

        public void Start()
        {
            throw new NotImplementedException("This is where the threading magic is going to happen.");
        }
    }

    public class ServiceRunnerServicePackage : IServicePackage
    {
        public void RegisterService(Container container)
        {
            container.RegisterConditional<ServiceRunner, ServiceRunner>(
                lifestyle: SimpleInjector.Lifestyle.Singleton
                , predicate: configuration => !configuration.Handled
            );
        }
    }

}
