using System.Security.Cryptography;
using System.Text;

namespace TradeBot.Engine.Exchange;

public sealed class ExchangeClient
{
    private readonly string _baseUrl;
    private readonly string _apiKey;
    private readonly string _apiSecret;
    private readonly Dictionary<string, SimulatedOrder> _orders = [];
    private int _orderId;

    public ExchangeClient(string baseUrl, string apiKey, string apiSecret)
    {
        _baseUrl = baseUrl;
        _apiKey = apiKey;
        _apiSecret = apiSecret;
    }

    public Task<decimal> GetPriceAsync(string symbol, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(symbol + DateTimeOffset.UtcNow.Hour));
        var price = 20000m + (hash[0] * 256 + hash[1]) / 10m;
        return Task.FromResult(Math.Round(price, 2));
    }

    public Task<decimal> GetBalanceAsync(string asset, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(_apiKey + asset));
        return Task.FromResult(Math.Round((hash[0] + 1) * 10m, 4));
    }

    public Task<string> PlaceOrderAsync(string symbol, string side, decimal qty, decimal? price, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var id = $"ORD-{Interlocked.Increment(ref _orderId):D6}";
        _orders[id] = new SimulatedOrder
        {
            Id = id, Symbol = symbol, Side = side,
            Quantity = qty, Price = price ?? 0,
            Status = "filled", FilledAt = DateTimeOffset.UtcNow
        };
        return Task.FromResult(id);
    }

    public Task<SimulatedOrder?> GetOrderAsync(string orderId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(_orders.GetValueOrDefault(orderId));
    }

    public IReadOnlyList<SimulatedOrder> OpenOrders => _orders.Values.Where(o => o.Status == "open").ToList();
    public IReadOnlyList<SimulatedOrder> AllOrders => _orders.Values.ToList();

    public string HmacSign(string payload)
    {
        var key = Encoding.UTF8.GetBytes(_apiSecret);
        var hash = HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

public sealed class SimulatedOrder
{
    public required string Id { get; init; }
    public required string Symbol { get; init; }
    public required string Side { get; init; }
    public decimal Quantity { get; init; }
    public decimal Price { get; init; }
    public string Status { get; set; } = "open";
    public DateTimeOffset? FilledAt { get; set; }
}
