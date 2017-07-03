using System.Collections.Generic;

namespace Vanderstack.Api.Core.Infrastructure.DependencyInjection
{
    public class CompositeServicePackage : IServicePackage, IComposite
    {
        public CompositeServicePackage(IEnumerable<IServicePackage> servicePackages)
        {
            _servicePackages = servicePackages;
        }

        private readonly IEnumerable<IServicePackage> _servicePackages;

        public void RegisterService(Container container)
        {
            foreach (var servicePackage in _servicePackages)
            {
                servicePackage.RegisterService(container);
            }
        }
    }
}
