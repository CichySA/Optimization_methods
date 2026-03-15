using PFSP.Instances;

namespace ExperimentRunner
{
    public static class ProblemLoader
    {
        public static Instance Load(string baseName) => InstanceReader.Read(baseName);

        public static List<(string Name, Instance Inst)> LoadMany(IEnumerable<string> baseNames)
        {
            var list = new List<(string, Instance)>();
            foreach (var name in baseNames)
            {
                try
                {
                    var inst = InstanceReader.Read(name);
                    list.Add((name, inst));
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
