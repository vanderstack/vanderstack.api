namespace Vanderstack.Api.Core.Infrastructure.DependencyInjection
{
    public class Container : SimpleInjector.Container, IContainer
    {
    }

    internal class ContainerStartupServicePackage : IStartupServiceObjectGraphConfiguration
    {
        public void RegisterStartupService(Container container)
        {
            container.RegisterConditional<Container, Container>(
                lifestyle: SimpleInjector.Lifestyle.Singleton
                , predicate: configuration => !configuration.Handled
            );
        }
    }
}
