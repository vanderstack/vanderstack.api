using Vanderstack.Api.Core.Infrastructure.DependencyInjection;

namespace Vanderstack.Api.Core
{
    public interface IServiceRunner
    {
        void Start();
    }

    public class IServiceRunnerPackage : IServiceGraphConfiguration
    {
        public void RegisterService(Container container)
        {
            container.RegisterConditional(
                serviceType: typeof(IServiceRunner)
                , registration: SimpleInjector.Lifestyle.Singleton.CreateRegistration(
                    () => container.GetInstance<ServiceRunner>()
                    , container
                )
                , predicate: configuration => !configuration.Handled
            );
        }
    }
}
