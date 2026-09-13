namespace TradeBot.Engine.Strategy;

public sealed class GridStrategy : BaseStrategy
{
    private readonly decimal _upperBound;
    private readonly decimal _lowerBound;
    private readonly int _gridCount;
    private readonly decimal _quantityPerGrid;

    public GridStrategy(decimal upper, decimal lower, int grids, decimal qty)
        : base("grid")
    {
        _upperBound = upper;
        _lowerBound = lower;
        _gridCount = Math.Max(2, grids);
        _quantityPerGrid = qty;
    }

    public IReadOnlyList<decimal> GridLevels()
    {
        var step = (_upperBound - _lowerBound) / (_gridCount - 1);
        return Enumerable.Range(0, _gridCount)
            .Select(i => Math.Round(_lowerBound + step * i, 2))
            .ToList();
    }

    public override IReadOnlyList<Signal> Evaluate(IReadOnlyList<Candle> candles)
    {
        if (candles.Count == 0) return [];
        var last = candles[^1];
        var signals = new List<Signal>();
        var levels = GridLevels();

        foreach (var level in levels)
        {
            if (last.Close <= level && last.Close >= level - (levels[1] - levels[0]))
            {
                signals.Add(new Signal
                {
                    Side = "buy",
                    Price = level,
                    Quantity = _quantityPerGrid,
                    Reason = $"grid buy at {level}"
                });
            }
            else if (last.Close >= level && last.Close <= level + (levels[1] - levels[0]))
            {
                signals.Add(new Signal
                {
                    Side = "sell",
                    Price = level,
                    Quantity = _quantityPerGrid,
                    Reason = $"grid sell at {level}"
                });
            }
        }

        return signals;
    }
}
