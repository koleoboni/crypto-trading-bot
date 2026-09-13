using TradeBot.Engine.Core;
using TradeBot.Engine.Strategy;

namespace TradeBot.Engine.Exchange;

public sealed class OrderRouter
{
    private readonly ExchangeClient _client;
    private readonly RiskManager _risk;
    private readonly List<TradeRecord> _trades = [];

    public OrderRouter(ExchangeClient client, RiskManager risk)
    {
        _client = client;
        _risk = risk;
    }

    public async Task<string?> RouteSignalAsync(Signal signal, decimal portfolioValue, CancellationToken ct = default)
    {
        var positionValue = signal.Price * signal.Quantity;
        if (signal.Side == "buy" && !_risk.CanOpenPosition(positionValue, portfolioValue))
        {
            Console.WriteLine($"[risk] blocked: {signal.Reason}");
            return null;
        }

        var orderId = await _client.PlaceOrderAsync(
            "BTCUSDT", signal.Side, signal.Quantity, signal.Price, ct);

        _trades.Add(new TradeRecord
        {
            OrderId = orderId,
            Side = signal.Side,
            Price = signal.Price,
            Quantity = signal.Quantity,
            Timestamp = DateTimeOffset.UtcNow
        });

        return orderId;
    }

    public IReadOnlyList<TradeRecord> TradeHistory => _trades;

    public decimal TotalPnl => _trades
        .Sum(t => t.Side == "sell" ? t.Price * t.Quantity : -(t.Price * t.Quantity));
}

public sealed class TradeRecord
{
    public required string OrderId { get; init; }
    public required string Side { get; init; }
    public decimal Price { get; init; }
    public decimal Quantity { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}
