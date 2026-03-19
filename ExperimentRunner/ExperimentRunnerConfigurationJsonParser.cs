using System.Text.Json;

namespace ExperimentRunner
{
    public static class ExperimentRunnerConfigurationJsonParser
    {
        public static ExperimentRunnerConfiguration Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Config file not found: {path}", path);

            var json = File.ReadAllText(path);
            var dto = JsonSerializer.Deserialize<ExperimentRunnerConfigurationDto>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new ExperimentRunnerConfigurationDto();

            return ExperimentRunnerConfiguration.From(dto.Instances, dto.Samples, dto.Seed, dto.OutDir);
        }

        private sealed class ExperimentRunnerConfigurationDto
        {
            public string[]? Instances { get; set; }
            public int[]? Samples { get; set; }
            public int? Seed { get; set; }
            public string? OutDir { get; set; }
        }
    }
}
