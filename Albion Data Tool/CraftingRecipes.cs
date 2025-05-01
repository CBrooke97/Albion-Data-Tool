using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Albion_Data_Tool
{
    public class RecipeBook
    {
        public Dictionary<string, Recipe> Recipes { get; set; }
        public Dictionary<string, List<string>> Categories { get; set; }
    }

    public class Recipe
    {
        public string PartialId { get; set; }
        public ComponentsMap Components { get; set; }
    }

    public class ComponentsMap
    {
        public List<Component> Basic { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement> Tiered { get; set; } // Holds "4", "5", etc. dynamically
    }

    public class Component
    {
        public string PartialId { get; set; }
        public int Amount { get; set; }
        public int TierOffset { get; set; }
    }
}
