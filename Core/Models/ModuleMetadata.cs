using Material.Icons;

namespace Core.Models
{
    public record struct ModuleMetadata(string RawName, string Name, bool AsButton = false, MaterialIconKind? Icon = MaterialIconKind.Help, int Position = 999)
    {
        public static ModuleMetadata Parse(string input)
        {
            string[] inputs = input.Split('#', StringSplitOptions.TrimEntries);
            return inputs.Length switch
            {
                4 => new ModuleMetadata(input, inputs[0], bool.Parse(inputs[1]), Enum.Parse<MaterialIconKind>(inputs[2]), int.Parse(inputs[3])),
                3 => new ModuleMetadata(input, inputs[0], bool.Parse(inputs[1]), Enum.Parse<MaterialIconKind>(inputs[2])),
                2 => new ModuleMetadata(input, inputs[0], bool.Parse(inputs[1])),
                _ => new ModuleMetadata(input, inputs[0])
            };
        }
    }
}
