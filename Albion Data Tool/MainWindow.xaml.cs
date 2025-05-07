using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;


namespace Albion_Data_Tool;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private static Dictionary<string, string> IdToNameMap = new Dictionary<string, string>();
    private static Dictionary<string, List<string>> NameToMultiIdMap = new Dictionary<string, List<string>>();
    private static Dictionary<string, AlbionMarketData> IdToMarketDataMap = new Dictionary<string, AlbionMarketData>();
    public MainWindow()
    {
        InitializeComponent();

        using FileStream fs = File.OpenRead("items.json");
        using JsonDocument itemsJsonDoc = JsonDocument.Parse(fs);

        foreach (JsonElement item in itemsJsonDoc.RootElement.EnumerateArray())
        {
            JsonElement idElement;
            JsonElement nameElement;

            if (!item.TryGetProperty("UniqueName", out idElement))
            {
                continue;
            }

            if (item.TryGetProperty("LocalizedNames", out JsonElement locNamesElement))
            {
                if (locNamesElement.ValueKind == JsonValueKind.Null || !locNamesElement.TryGetProperty("EN-US", out nameElement))
                {
                    continue;
                }
            }
            else
            {
                continue;
            }

            string ItemId = idElement.GetString();
            string ItemName = nameElement.GetString();

            IdToNameMap.Add(ItemId, ItemName);

            if (NameToMultiIdMap.ContainsKey(ItemName))
            {
                NameToMultiIdMap[ItemName].Add(ItemId);
            }
            else
            {
                NameToMultiIdMap.Add(ItemName, new List<String> { ItemId });
            }
        }


        using var recipeDoc = JsonDocument.Parse(File.ReadAllText("recipes.json"));
        var rootElem = recipeDoc.RootElement;

        RecipeBook recipeBook = new RecipeBook();

        // Iterate through each recipe in the "Recipes" section
        foreach (JsonProperty recipeProp in rootElem.GetProperty("Recipes").EnumerateObject())
        {
            JsonElement recipeElement = recipeProp.Value;

            // Try to get the PartialId
            JsonElement idElement;
            if (!recipeElement.TryGetProperty("PartialId", out idElement))
            {
                continue; // Skip if no PartialId is found
            }

            string? partialId = idElement.GetString();
            if (partialId == null)
            {
                continue; // Skip if PartialId is null
            }

            // Try to get the Components section
            JsonElement ComponentsElement;
            if (!recipeElement.TryGetProperty("Components", out ComponentsElement))
            {
                continue; // Skip if no Components are found
            }

            Recipe recipe = new Recipe();

            recipe.PartialId = partialId;

            // Iterate through the components by tier
            foreach (var ComponentProperty in ComponentsElement.EnumerateObject())
            {
                string componentTier = ComponentProperty.Name;

                // Deserialize the component list for the current tier
                var components = JsonSerializer.Deserialize<List<Component>>(ComponentProperty.Value.GetRawText());

                // Skip if deserialization fails (null result)
                if (components == null)
                {
                    continue;
                }

                // Add the components to the recipe under the appropriate tier
                recipe.Add(componentTier, components);
            }

            // Add the recipe to the recipe book
            recipeBook.Add(recipeProp.Name, recipe);
        }

        Recipe targetRecipe = recipeBook["Cloth"];

        List<string> itemIds = new List<string>();

        for (int tier = 3; tier <= 8; tier++)
        {
            string productItemId = "T" + tier.ToString() + "_";

            productItemId += targetRecipe.PartialId;

            itemIds.Add(productItemId);

            if (targetRecipe.TryGetValue("Basic", out List<Component>? basicComponents))
            {
                foreach (var component in basicComponents)
                {
                    string componentItemId = "T" + (tier + component.TierOffset).ToString() + "_";
                    componentItemId += component.PartialId;

                    itemIds.Add(componentItemId);
                }
            }

            if (targetRecipe.TryGetValue(tier.ToString(), out List<Component>? tierComponents))
            {
                foreach (var component in tierComponents)
                {
                    string componentItemId = "T" + (tier + component.TierOffset).ToString() + "_";
                    componentItemId += component.PartialId;

                    itemIds.Add(componentItemId);
                }
            }
        }

        _ = LoadDataAsync(itemIds);
    }

    private async Task LoadDataAsync(List<string> itemIds)
    {
        HttpClient client = new HttpClient();

        string baseUrl = $"https://east.albion-online-data.com/api/v2/stats/prices/";

        string joinedIds = string.Join(",", itemIds);

        string apiQueryURL = $"{baseUrl}{joinedIds}.json";

        try
        {
            string json = await client.GetStringAsync(apiQueryURL);

            using JsonDocument jsonDoc = JsonDocument.Parse(json);

            foreach (JsonElement item in jsonDoc.RootElement.EnumerateArray())
            {
                JsonElement idElement;
                if (!item.TryGetProperty("item_id", out idElement))
                {
                    continue;
                }

                string itemId = idElement.GetString();

                if (string.IsNullOrEmpty(itemId))
                {
                    continue;
                }

                JsonElement cityElement;
                if (!item.TryGetProperty("city", out cityElement))
                {
                    continue;
                }

                string cityName = cityElement.GetString();

                if (string.IsNullOrEmpty(cityName))
                {
                    continue;
                }

                JsonElement qualityElement;
                if (!item.TryGetProperty("quality", out qualityElement))
                {
                    continue;
                }

                int itemQuality = qualityElement.GetInt32();

                string key = $"{itemId}:{cityName}:{itemQuality}";

                AlbionMarketData marketData = new AlbionMarketData
                {
                    SellPriceMin = item.GetProperty("sell_price_min").GetInt32(),
                    SellPriceMinDate = item.GetProperty("sell_price_min_date").GetDateTime(),
                    SellPriceMax = item.GetProperty("sell_price_max").GetInt32(),
                    SellPriceMaxDate = item.GetProperty("sell_price_max_date").GetDateTime(),
                    BuyPriceMin = item.GetProperty("buy_price_min").GetInt32(),
                    BuyPriceMinDate = item.GetProperty("buy_price_min_date").GetDateTime(),
                    BuyPriceMax = item.GetProperty("buy_price_max").GetInt32(),
                    BuyPriceMaxDate = item.GetProperty("buy_price_max_date").GetDateTime(),
                };

                IdToMarketDataMap.Add(key, marketData);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching data: {ex.Message}");
        }
    }
}

public struct AlbionMarketData
{
    [JsonPropertyName("sell_price_min")]
    public int SellPriceMin { get; set; }

    [JsonPropertyName("sell_price_min_date")]
    public DateTime SellPriceMinDate { get; set; }

    [JsonPropertyName("sell_price_max")]
    public int SellPriceMax { get; set; }

    [JsonPropertyName("sell_price_max_date")]
    public DateTime SellPriceMaxDate { get; set; }

    [JsonPropertyName("buy_price_min")]
    public int BuyPriceMin { get; set; }

    [JsonPropertyName("buy_price_min_date")]
    public DateTime BuyPriceMinDate { get; set; }

    [JsonPropertyName("buy_price_max")]
    public int BuyPriceMax { get; set; }

    [JsonPropertyName("buy_price_max_date")]
    public DateTime BuyPriceMaxDate { get; set; }
}

