using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;

namespace Albion_Data_Tool;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        HttpClient client = new HttpClient();
        string json = await client.GetStringAsync($"https://east.albion-online-data.com/api/v2/stats/prices/T4_BAG,T5_BAG.json");

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        AlbionMarketEntry[] MarketEntry = JsonSerializer.Deserialize<AlbionMarketEntry[]>(json, options);

        foreach (var Entry in MarketEntry)
        {
            Console.WriteLine($"City: {Entry.City}, Min Sell Price: {Entry.SellPriceMin}");

            TextBox NewText = new TextBox();
            NewText.Text = Entry.City;
            WrapPanel.Children.Add(NewText);
        }
    }
}

public class AlbionMarketEntry
{
    [JsonPropertyName("item_id")]
    public string ItemId { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; }

    [JsonPropertyName("quality")]
    public int Quality { get; set; }

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

