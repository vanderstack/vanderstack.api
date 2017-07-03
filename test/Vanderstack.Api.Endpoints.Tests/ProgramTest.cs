using Xunit;

namespace Vanderstack.Api.Endpoints.Tests
{
    public class ProgramTest
    {
        [Fact]
        public void ProgramStarts()
        {
            Program.Main(new string[] { });
        }
    }
}
