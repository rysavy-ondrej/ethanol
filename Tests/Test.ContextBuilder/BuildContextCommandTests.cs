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