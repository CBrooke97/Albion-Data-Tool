namespace Albion_Data_Tool
{
    public class RecipeBook : Dictionary<string, Recipe>
    {

    }
    public class Recipe : Dictionary<string, List<Component>>
    {
        public string PartialId { get; set; }
    }

    public class Component
    {
        public string PartialId { get; set; }
        public int Amount { get; set; }
        public int TierOffset { get; set; }
    }
}
