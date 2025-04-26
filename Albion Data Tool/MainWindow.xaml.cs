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

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        HttpClient client = new HttpClient();

        try
        {
            string json = await client.GetStringAsync($"https://east.albion-online-data.com/api/v2/stats/prices/T4_BAG,T5_BAG.json");

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

