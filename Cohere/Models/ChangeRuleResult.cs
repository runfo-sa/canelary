namespace Cohere.Models
{
    public class ChangeRuleResult(bool remove, int? rule)
    {
        public bool Remove => remove;
        public int? Rule => rule;
    }
}