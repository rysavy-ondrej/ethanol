using Ethanol.ContextBuilder;

namespace Test.ContextBuilder
{
    /// <summary>
    /// This class tests the entire commands.
    /// </summary>
    public class BuildContextCommandTests
    {
        ProgramCommands commands;
        [SetUp]
        public void Setup()
        {
            commands = new ProgramCommands();
        }

        [Test]
        public async Task NfdumpInputYamlOutput()
        {
            await commands.BuildContextCommand("NfdumpCsv:{file=TestInput/short.nfdump.csv}", "TestConfig/config.yaml", "YamlWriter:{file=TestOutput/short.context.yaml}");
            Assert.Pass();
        }
        [Test]
        public void NfdumpInputJsonOutput()
        {
            Assert.Pass();
        }
        [Test]
        public void FlowexpInputYamlOutput()
        {
            Assert.Pass();
        }
        [Test]
        public void FlowexpInputJsonOutput()
        {
            Assert.Pass();
        }
    }
}