namespace Cohere.Models
{
    public class ChangeRuleResult(bool remove, string rule)
    {
        public bool Remove => remove;
        public string Rule => rule;
    }
}
