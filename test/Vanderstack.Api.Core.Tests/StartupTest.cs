using Xunit;

namespace Vanderstack.Api.Core.Tests
{
    public class StartupTest
    {
        [Fact]
        public void Startup_Starts()
        {
            new Launcher().launch();
        }
    }
}
