using FluentAssertions;
using Xunit;

namespace Minitwit.Test
{
    public class UnitTest1
    {
        [Fact]
        public void DummyTest()
        {
            true.Should().Be(true);
        }
    }
}