using ExperimentRunner;

namespace PfspTests.ExperimentRunner.Configuration
{
    [Trait("Area", "ExperimentRunner")]
    [Trait("Component", "Configuration")]
    [Trait("Kind", "Unit")]
    public class CliParserTests
    {
        private static string WriteTemp(string json)
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, json);
            return path;
        }

        private static string MinimalConfigPath() =>
            WriteTemp("""{ "Instances": ["tai_20_5_0"], "OutDir": "out", "Algorithms": [] }""");

        [Fact]
        public void Parse_EmptyArgs_ReturnsNull()
        {
            Assert.Null(ExperimentRunnerConfigurationCliParser.Parse([]));
        }

        [Theory]
        [InlineData("--help")]
        [InlineData("-h")]
        public void Parse_HelpFlag_ReturnsNull(string flag)
        {
            Assert.Null(ExperimentRunnerConfigurationCliParser.Parse([flag]));
        }

        [Fact]
        public void Parse_ValidConfigPath_LoadsConfiguration()
        {
            var path = MinimalConfigPath();

            var config = ExperimentRunnerConfigurationCliParser.Parse(["--config", path]);

            Assert.NotNull(config);
            Assert.Single(config!.Instances);
            Assert.Equal("tai_20_5_0", config.Instances[0]);
            Assert.Equal("out", config.OutDir);
            Assert.Empty(config.Algorithms);
        }

        [Fact]
        public void Parse_ConfigFlagIsCaseInsensitive()
        {
            var config = ExperimentRunnerConfigurationCliParser.Parse(["--CONFIG", MinimalConfigPath()]);
            Assert.NotNull(config);
        }

        [Fact]
        public void Parse_UnknownFlag_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                ExperimentRunnerConfigurationCliParser.Parse(["--unknown"]));
        }

        [Fact]
        public void Parse_ConfigFlagWithNoValue_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                ExperimentRunnerConfigurationCliParser.Parse(["--config"]));
        }

        [Fact]
        public void Parse_ConfigPathNotFound_ThrowsFileNotFoundException()
        {
            Assert.Throws<FileNotFoundException>(() =>
                ExperimentRunnerConfigurationCliParser.Parse(["--config", "not_a_real_file.json"]));
        }

        [Fact]
        public void Parse_ExtraFlagAfterConfig_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                ExperimentRunnerConfigurationCliParser.Parse(["--config", MinimalConfigPath(), "--extra"]));
        }
    }
}
