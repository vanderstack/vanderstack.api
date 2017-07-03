using Vanderstack.Api.Core.Infrastructure.Internal;

namespace Vanderstack.Api.Core
{
    public class Launcher
    {
        public Launcher() : this(new StartupService())
        {
        }

        internal Launcher(StartupService startupService)
        {
            _serviceRunner = startupService.ServiceRunner;
        }

        private readonly IServiceRunner _serviceRunner;

        public void launch()
        {
            _serviceRunner.Start();
        }
    }
}
