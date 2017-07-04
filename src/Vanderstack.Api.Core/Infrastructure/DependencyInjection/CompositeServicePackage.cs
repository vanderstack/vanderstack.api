using System.Collections.Generic;

namespace Vanderstack.Api.Core.Infrastructure.DependencyInjection
{
    public class CompositeServicePackage : IServiceGraphConfiguration, IComposite
    {
        public CompositeServicePackage(IEnumerable<IServiceGraphConfiguration> servicePackages)
        {
            _servicePackages = servicePackages;
        }

        private readonly IEnumerable<IServiceGraphConfiguration> _servicePackages;

        public void RegisterService(Container container)
        {
            foreach (var servicePackage in _servicePackages)
            {
                servicePackage.RegisterService(container);
            }
        }
    }
}
