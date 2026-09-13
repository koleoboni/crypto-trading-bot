using System.Text.Json;

namespace TradeBot.Engine.Core;

public sealed class BotConfig
{
    public string Exchange { get; set; } = "binance";
    public string ApiKey { get; set; } = "";
    public string ApiSecret { get; set; } = "";
    public string Symbol { get; set; } = "BTCUSDT";
    public string Mode { get; set; } = "paper";
    public decimal InitialCapital { get; set; } = 10000m;
    public decimal MaxPositionPct { get; set; } = 0.1m;
    public decimal MaxDailyLossPct { get; set; } = 0.05m;
    public string Strategy { get; set; } = "grid";
    public int BacktestDays { get; set; } = 30;
    public string TimeFrame { get; set; } = "1h";
    public bool EnableWebSocket { get; set; } = true;

    public static BotConfig LoadOrDefault(string path)
    {
        if (!File.Exists(path)) return new BotConfig();
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<BotConfig>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }) ?? new BotConfig();
    }

    public void Save(string path)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
