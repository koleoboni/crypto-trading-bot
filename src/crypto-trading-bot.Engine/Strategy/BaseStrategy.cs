namespace TradeBot.Engine.Strategy;

public abstract class BaseStrategy
{
    public string Name { get; }
    protected BaseStrategy(string name) => Name = name;

    public abstract IReadOnlyList<Signal> Evaluate(IReadOnlyList<Candle> candles);

    public virtual void Reset() { }
}

public sealed class Signal
{
    public required string Side { get; init; }
    public decimal Price { get; init; }
    public decimal Quantity { get; init; }
    public string? Reason { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}

public sealed class Candle
{
    public DateTimeOffset Time { get; init; }
    public decimal Open { get; init; }
    public decimal High { get; init; }
    public decimal Low { get; init; }
    public decimal Close { get; init; }
    public decimal Volume { get; init; }
}
