using Core.Database.Model;

namespace Cohere.Models
{
    public class CreateRuleResult(string label, string rule, IEnumerable<ReglaAtributo> attributes)
    {
        public string Label => label;
        public string Rule => rule;
        public IEnumerable<ReglaAtributo> Attributes => attributes;
    }
}
