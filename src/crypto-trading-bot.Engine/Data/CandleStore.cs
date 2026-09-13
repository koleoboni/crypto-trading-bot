using System.Security.Cryptography;
using System.Text;
using TradeBot.Engine.Strategy;

namespace TradeBot.Engine.Data;

public sealed class CandleStore
{
    private readonly Dictionary<string, List<Candle>> _data = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<Candle> Get(string symbol, string timeframe, int limit = 100)
    {
        var key = $"{symbol}:{timeframe}";
        if (_data.TryGetValue(key, out var list))
            return list.TakeLast(limit).ToList();
        return GenerateSimulated(symbol, timeframe, limit);
    }

    public void Append(string symbol, string timeframe, Candle candle)
    {
        var key = $"{symbol}:{timeframe}";
        if (!_data.ContainsKey(key)) _data[key] = [];
        _data[key].Add(candle);
        if (_data[key].Count > 5000) _data[key].RemoveRange(0, 1000);
    }

    private static List<Candle> GenerateSimulated(string symbol, string timeframe, int count)
    {
        var result = new List<Candle>(count);
        var price = 30000m;
        var tfMinutes = timeframe switch
        {
            "1m" => 1, "5m" => 5, "15m" => 15, "1h" => 60, "4h" => 240, "1d" => 1440, _ => 60
        };

        for (var i = 0; i < count; i++)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{symbol}:{timeframe}:{i}"));
            var delta = (hash[0] - 128) / 50m;
            price += delta;
            if (price < 100) price = 100;
            result.Add(new Candle
            {
                Time = DateTimeOffset.UtcNow.AddMinutes(-tfMinutes * (count - i)),
                Open = price,
                High = price + Math.Abs(delta) * 2,
                Low = price - Math.Abs(delta) * 2,
                Close = price + delta / 2,
                Volume = (hash[1] + 1) * 50m
            });
        }
        return result;
    }
}
