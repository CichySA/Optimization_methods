using PFSP.Instances;

namespace ExperimentRunner
{
    public static class ProblemLoader
    {
        public static Instance Load(string baseNameOrPath) => InstanceReader.Read(PathResolver.NormalizeInstanceIdentifier(baseNameOrPath));

        public static List<(string Name, Instance Inst)> LoadMany(IEnumerable<string> baseNamesOrPaths)
        {
            var list = new List<(string, Instance)>();
            foreach (var name in baseNamesOrPaths)
            {
                try
                {
                    var normalizedName = PathResolver.NormalizeInstanceIdentifier(name);
                    var inst = InstanceReader.Read(normalizedName);
                    list.Add((normalizedName, inst));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: failed to load instance '{name}': {ex.Message}");
                }
            }
            return list;
        }
    }
}
