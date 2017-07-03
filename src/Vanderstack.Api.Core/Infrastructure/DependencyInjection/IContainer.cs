namespace Vanderstack.Api.Core.Infrastructure.DependencyInjection
{
    public interface IContainer
    {
        TService GetInstance<TService>() where TService : class;
    }

    internal class IContainerStartupServicePackage : IStartupServicePackage
    {
        public void RegisterStartupService(Container container)
        {
            container.RegisterConditional(
                serviceType: typeof(IContainer)
                , registration: SimpleInjector.Lifestyle.Singleton.CreateRegistration(
                    () => container.GetInstance<InitializedContainerProxy>()
                    , container
                )
                , predicate: configuration => !configuration.Handled
            );
        }
    }
}
