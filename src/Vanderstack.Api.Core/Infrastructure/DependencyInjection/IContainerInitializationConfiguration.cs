using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Vanderstack.Api.Core.Infrastructure.DependencyInjection
{
    public class Program
    {
        public void Main()
        {
            Container container = new Container();
            container.Options.DefaultLifestyle = Lifestyle.Scoped;
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            var exceptionManager = new ApplicationExceptionManager();
            container.RegisterSingleton(instance: exceptionManager);

            container.Register<ApplicationLauncher>();

            container.Verify();

            while (false == exceptionManager.ShutdownRequired)
            {
                try
                {
                    using (AsyncScopedLifestyle.BeginScope(container))
                    {
                        container.GetInstance<ApplicationLauncher>().Launch();
                    }
                }
                catch (Exception exception)
                {
                    exceptionManager.RegisterException(exception);
                }
            }
        }
    }

    public class ApplicationLauncher
    {
        public ApplicationLauncher(
            RuntimeContainerProvider runtimeContainerProvider
        )
        {
            _runtimeContainerProvider = runtimeContainerProvider;
        }

        private readonly RuntimeContainerProvider _runtimeContainerProvider;

        public void Launch()
        {
            _runtimeContainerProvider
                .Container
                .GetInstance<IMicroServiceLauncher>()
                .Launch();
        }
    }

    public class RuntimeContainerProvider
    {
        public RuntimeContainerProvider(
            RuntimeContainer container
            , RuntimeContainerInitializer containerInitializer
        )
        {
            containerInitializer.Initialize(container);
            Container = container;
        }

        public RuntimeContainer Container { get; }
    }

    public class RuntimeContainer : Container
    {

    }

    public interface IConfigureObjectGraph
    {
        void Configure(RuntimeContainer container, PlatformContext platformContext);
    }

    public interface IConfigureObjectGraph<TService>
        : IConfigureObjectGraph
    {
    }

    public class AssemblyProvider
    {
        public IEnumerable<Assembly> Assemblies =>
            throw new NotImplementedException();
    }

    public interface IApplicationSetting
    {

    }

    public class ApplicationSettings
    {
        public ApplicationSettings(AssemblyProvider assemblyProvider)
        {
            var applicationSettingsContainer = new Container();
            applicationSettingsContainer
                .RegisterCollection<IApplicationSetting>(
                    assemblyProvider.Assemblies
                );

            _applicationSettings = applicationSettingsContainer.GetInstance<IEnumerable<IApplicationSetting>>();
        }

        private readonly IEnumerable<IApplicationSetting> _applicationSettings;

        public TSetting For<TSetting>() where TSetting : IApplicationSetting
        {
            return _applicationSettings.OfType<TSetting>().Single();
        }
    }

    public class EnvironmentSettings : IApplicationSetting
    {
        public string EnvironmentName { get; }
    }

    public class PlatformContext
    {
        public PlatformContext(
            AssemblyProvider assemblyProvider
            , ApplicationSettings applicationSettings
            , ApplicationExceptionManager applicationExceptionManager
        )
        {
            Assemblies = assemblyProvider.Assemblies;
            ApplicationSettings = applicationSettings;
            ApplicationExceptionManager = applicationExceptionManager;
        }

        public IEnumerable<Assembly> Assemblies { get; }

        public ApplicationSettings ApplicationSettings { get; }

        public ApplicationExceptionManager ApplicationExceptionManager { get; }
    }

    public class ObjectGraphConfigurationManager
    {
        public ObjectGraphConfigurationManager(PlatformContext platformContext)
        {
            _platformContext = platformContext;

            var objectGraphConfigurationContainer = new Container();
            objectGraphConfigurationContainer
                .RegisterCollection(
                    typeof(IConfigureObjectGraph)
                    , _platformContext.Assemblies
                );

            _objectGraphConfigurations =
                objectGraphConfigurationContainer
                .GetInstance<IEnumerable<IConfigureObjectGraph>>();
        }

        private readonly PlatformContext _platformContext;
        private readonly IEnumerable<IConfigureObjectGraph> _objectGraphConfigurations;

        public void ConfigureObjectGraph(RuntimeContainer container)
        {
            foreach (var iConfigureObjectGraph in _objectGraphConfigurations)
            {
                iConfigureObjectGraph.Configure(container, _platformContext);
            }
        }
    }

    public class PlatformContextGraphConfiguration : IConfigureObjectGraph<PlatformContext>
    {
        public void Configure(RuntimeContainer container, PlatformContext platformContext)
        {
            container.RegisterSingleton<PlatformContext>(platformContext);
        }
    }

    public class RuntimeContainerGraphConfiguration : IConfigureObjectGraph<RuntimeContainer>
    {
        public void Configure(RuntimeContainer container, PlatformContext platformContext)
        {
            container.RegisterSingleton<RuntimeContainer>(container);
        }
    }



    public class RuntimeContainerInitializer
    {
        public RuntimeContainerInitializer(
            ObjectGraphConfigurationManager blockConfigurationManager
        )
        {
            _objectGraphConfigurationManager = blockConfigurationManager;
        }

        private readonly ObjectGraphConfigurationManager _objectGraphConfigurationManager;

        public void Initialize(RuntimeContainer container)
        {
            container.Options.DefaultLifestyle = Lifestyle.Scoped;
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            _objectGraphConfigurationManager.ConfigureObjectGraph(container);

            container.Verify();
        }
    }

    public interface IMicroServiceLauncher
    {
        void Launch();
    }

    public class ApplicationExceptionManager
    {
        public ApplicationExceptionManager()
        {
            _exceptions = new List<Exception>();
        }

        private readonly ICollection<Exception> _exceptions;

        public bool ShutdownRequired { get; private set; } = false;

        public void Shutdown() => ShutdownRequired = true;

        public void RegisterException(Exception exception)
        {
            _exceptions.Add(exception);
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
