using System;
using SimpleInjector;
using System.Collections.Generic;
using SimpleInjector.Lifestyles;
using System.Reflection;

namespace Vanderstack.Api.Core.Infrastructure.DependencyInjection
{
    public class TestProgram
    {
        public void Main()
        {
            Container container = new Container();
            container.Options.DefaultLifestyle = Lifestyle.Scoped;
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            bool doEventLoop = true;
            container.RegisterSingleton<ApplicationShutdownManager>(
                new ApplicationShutdownManager(
                    shutdownAction: () => doEventLoop = false
                )
            );

            ICollection<Exception> exceptions = new List<Exception>();
            container.RegisterSingleton<ICollection<Exception>>(exceptions);

            container.Register<TestProgramLauncher>(Lifestyle.Scoped);

            container.Verify();

            while (doEventLoop)
            {
                try
                {
                    using (AsyncScopedLifestyle.BeginScope(container))
                    {
                        container.GetInstance<TestProgramLauncher>().Launch();
                    }
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                }
            }
        }
    }

    public class TestProgramLauncher
    {
        public TestProgramLauncher(
            RuntimeConfiguredContainer container
        )
        {
            _container = container;
            /*

                        ICollection<Exception> exceptions
            , ApplicationShutdownManager shutdownManager
            , RuntimeConfiguredContainer container
            // These should be part of the container initializer, which should be inside the runtime configured container
            , ApplicationSettingsProvider applicationSettingsProvider
            , AssemblyProvider assemblyProvider
            // literally anything else you would normally new up

            */
        }

        private readonly RuntimeConfiguredContainer _container;

        public void Launch()
        {
            _container.GetInstance<IMicroServiceLauncher>().Launch();

            // if our inner container verification fails or we have errors which we cannot handle, we can log them and them and then invoke the shutdown manager
            shutdownManager.Shutdown();
        }
    }

    public class RuntimeConfiguredContainer
    {
        public RuntimeConfiguredContainer(
            RuntimeContainer container
            , RuntimeContainerInitializer containerInitializer
        )
        {
            containerInitializer.InitializeContainer(container);
            _container = container;
        }

        private readonly RuntimeContainer _container;

        public TService GetInstance<TService>() where TService : class
        {
            return _container.GetInstance<TService>();
        }
    }

    public class RuntimeContainer : Container
    {

    }

    public interface IConfigureBlocks
    {
        void Configure(RuntimeContainer container, ApplicationSettings applicationSettings, IEnumerable<Assembly> assemblies);
    }

    public class PlatformContext
    {
        public IEnumerable<Assembly> Assemblies { get; }
        public IApplicationSettings ApplicationSettings { get; }
        public ICollection<Exception> Exceptions { get; }
    }

    public class BlockConfigurationManager
    {
        public BlockConfigurationManager(PlatformContext context)
        {
            _context = context;

            var blockConfigurationContainer = new Container();
            blockConfigurationContainer.RegisterCollection<IConfigureBlocks>(context.Assemblies);
            _blockConfigurations = blockConfigurationContainer.GetInstance<IEnumerable<IConfigureBlocks>>();
        }

        private readonly IEnumerable<IConfigureBlocks> _blockConfigurations;
        private readonly PlatformContext _context;

        public void ConfigureBlocks(RuntimeContainer container)
        {
            foreach (var configuration in _blockConfigurations)
            {
                configuration.Configure(container, _context.ApplicationSettings, _context.Assemblies);
            }
        }
    }

    public class RuntimeContainerInitializer
    {
        public RuntimeContainerInitializer(
            BlockConfigurationManager blockConfigurationManager
        )
        {
            _blockConfigurationManager = blockConfigurationManager;
        }

        private readonly BlockConfigurationManager _blockConfigurationManager;

        /*

ICollection<Exception> exceptions
, ApplicationShutdownManager shutdownManager
, RuntimeConfiguredContainer container
// These should be part of the container initializer, which should be inside the runtime configured container
, ApplicationSettingsProvider applicationSettingsProvider
, AssemblyProvider assemblyProvider
// literally anything else you would normally new up

*/
        public void InitializeContainer(RuntimeContainer container)
        {
            container.Options.DefaultLifestyle = Lifestyle.Scoped;
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            // keep going with initializing the Runtime Container with services
            // container.register IHttpClient etc
            // container.register IMicroServiceLauncher
            // these should be done with IRegisterBlocks which is like IPackage but for vanderstack components.
            // container.RegisterSingleton<IEnumerable<Assembly>>(_assemblyProvider.Assemblies);
            // container register singleton appsettings
            // we have assembly provider. we should use it to register a container with all assemblies to get IEnumerable IRegisterBlocks

            _blockConfigurationManager.ConfigureBlocks(container);
            

            container.Verify();
        }
    }

    public interface IMicroServiceLauncher
    {
        void Launch();
    }

    public class ApplicationShutdownManager
    {
        public ApplicationShutdownManager(Action shutdownAction)
        {
            _shutdownAction = shutdownAction;
        }

        private readonly Action _shutdownAction;

        public void Shutdown()
        {
            _shutdownAction();
        }
    }

































    public interface IContainerInitializationConfiguration : IStartupService
    {
        Lifestyle DefaultLifestyle { get; }
        ScopedLifestyle DefaultScopedLifestyle { get; }
        bool RequireVerification { get; }
        IObjectGraphConfiguration ObjectGraphConfiguration { get; }
    }

    public interface IApplicationLifecycle
    {

    }

    /// <summary>
    /// The first stage in the application lifecycle.
    /// </summary>
    ///
    /// <remarks>
    /// At this point we are not able to take a dependency on anything which is not provided by .NET
    /// We configure the object graph so that during the ApplicationStartup Lifecycle the startup
    /// process is able to take dependencies on 
    ///   Application configuration
    ///   Assemblies
    ///     Api Defined
    ///     User Defined (Depends on Api Defined Assembly)
    ///       Defined in DependencyContext.Runtime.Default
    ///       Deployed to same folder
    ///   IStartupService Object Graph Configuration(s) 
    /// </remarks>
    public interface IApplicationConfiguration : IApplicationLifecycle
    {
    }

    /// <summary>
    /// The second stage in the application lifecycle.
    /// </summary>
    ///
    /// <remarks>
    /// This is where we configure the object graph for Application Started Dependencies
    /// </remarks>
    public interface IApplicationStartup : IApplicationLifecycle
    {

    }

    /// <summary>
    /// 
    /// </summary>
    public interface IApplicationStarted : IApplicationLifecycle
    {

    }

    public interface IService<in TApplicationLifecycle>
        : IService
        where TApplicationLifecycle : IApplicationLifecycle
    {

    }

    // A Service which encapsulates behavior or state the library or user code may depend on
    public interface IService
    {

    }

    // An Service which encapsulates system / platform / code level state or behavior.
    // These services are available during the startup application lifecycle.
    // This often includes the assembly provider, the type provider, and all system configuration values
    public interface ISystemService : IService<IApplicationConfiguration>
    {

    }

    // A Service which encapsulates startup level state or behavior.
    // This information is 
    public interface IStartupService : IService
    {

    }

    public interface IRuntimeService : IService
    {

    }

    public interface IObjectGraphConfiguration
    {
        void ConfigureObjectGraph(Container container);
        SimpleInjector.Lifestyle Lifestyle { get; }
    }

    public interface IObjectGraphConfiguration<out TService>
        : IObjectGraphConfiguration
        where TService : IService
    {

    }

    public interface IRuntimeServiceObjectGraphConfiguration<out TRuntimeService>
        : IObjectGraphConfiguration<TRuntimeService>
        where TRuntimeService : IRuntimeService
    {

    }

    public interface IStartupServiceObjectGraphConfiguration<out TStartupService>
        : IObjectGraphConfiguration<TStartupService>
        where TStartupService : IStartupService
    {

    }

    // This will initialize the lowest level container which is responsible
    // for returning configuration values related to the application environment
    // and other compile time known features.
    // this will include IEnumerable<Assembly>, IEnumerable<Type>, strongly typed Configuration, etc
    public interface ISystemServiceObjectGraphConfiguration<out TSystemService>
        : IObjectGraphConfiguration<TSystemService>
        where TSystemService : ISystemService
    {

    }

    public interface IDefaultImplementationGraphConfiguration<out TService, out TServiceImplementation>
        : IObjectGraphConfiguration<TService>
        where TService : IService
        where TServiceImplementation : class, TService
    {
        Func<TServiceImplementation> GetInstanceCreator(Container container);
        Registration GetRegistration(Container container);
    }

    public abstract class DefaultImplementationGraphConfiguration<TService, TServiceImplementation>
        : IDefaultImplementationGraphConfiguration<TService, TServiceImplementation>
        where TService : IService
        where TServiceImplementation : class, TService
    {
        public abstract Lifestyle Lifestyle { get; }

        public void ConfigureObjectGraph(Container container)
        {
            container.RegisterConditional(
                serviceType: typeof(TService)
                , registration: GetRegistration(container)
                , predicate: configuration => !configuration.Handled
            );
        }

        public virtual Registration GetRegistration(Container container) =>
            Lifestyle.CreateRegistration(GetInstanceCreator(container), container);

        public virtual Func<TServiceImplementation> GetInstanceCreator(Container container) =>
            () => container.GetInstance<TServiceImplementation>();
    }

    public abstract class StartupServiceDefaultImplementationGraphConfiguration<TRuntimeService, TRuntimeServiceImplementation>
        : DefaultImplementationGraphConfiguration<TRuntimeService, TRuntimeServiceImplementation>
        , IStartupServiceObjectGraphConfiguration<TRuntimeService>
        where TRuntimeService : IStartupService
        where TRuntimeServiceImplementation : class, TRuntimeService
    {

    }


    public abstract class SystemServiceDefaultImplementationGraphConfiguration<TSystemService, TSystemServiceImplementation>
        : DefaultImplementationGraphConfiguration<TSystemService, TSystemServiceImplementation>
        , ISystemServiceObjectGraphConfiguration<TSystemService>
        where TSystemService : ISystemService
        where TSystemServiceImplementation : class, TSystemService
    {

    }


    public abstract class RuntimeServiceDefaultImplementationGraphConfiguration<TRuntimeService, TRuntimeServiceImplementation>
        : DefaultImplementationGraphConfiguration<TRuntimeService, TRuntimeServiceImplementation>
        , IRuntimeServiceObjectGraphConfiguration<TRuntimeService>
        where TRuntimeService : IRuntimeService
        where TRuntimeServiceImplementation : class, TRuntimeService
    {

    }

    public class IContainerInitializationConfigurationPackage
        : StartupServiceDefaultImplementationGraphConfiguration<IContainerInitializationConfiguration, ContainerInitializationConfiguration>
    {
        public override Lifestyle Lifestyle => Lifestyle.Transient;
    }
}
