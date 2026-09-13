namespace TradeBot.Engine.Strategy;

public sealed class DcaStrategy : BaseStrategy
{
    private readonly decimal _buyAmount;
    private readonly decimal _dipMultiplier;
    private decimal _averageCost;
    private int _buyCount;

    public DcaStrategy(decimal buyAmount, decimal dipMultiplier = 1.5m)
        : base("dca")
    {
        _buyAmount = buyAmount;
        _dipMultiplier = dipMultiplier;
    }

    public override IReadOnlyList<Signal> Evaluate(IReadOnlyList<Candle> candles)
    {
        if (candles.Count < 2) return [];
        var last = candles[^1];
        var prev = candles[^2];
        var changePct = prev.Close == 0 ? 0 : (last.Close - prev.Close) / prev.Close * 100m;
        var qty = _buyAmount;

        if (changePct < -5) qty *= _dipMultiplier;
        if (changePct < -10) qty *= _dipMultiplier;

        _buyCount++;
        _averageCost = _averageCost == 0
            ? last.Close
            : (_averageCost * (_buyCount - 1) + last.Close) / _buyCount;

        return
        [
            new Signal
            {
                Side = "buy",
                Price = last.Close,
                Quantity = qty,
                Reason = $"dca buy #{_buyCount}, avg={_averageCost:F2}, change={changePct:F1}%"
            }
        ];
    }

    public override void Reset()
    {
        _averageCost = 0;
        _buyCount = 0;
    }

    public decimal AverageCost => _averageCost;
    public int BuyCount => _buyCount;
}

public sealed class EmaCrossStrategy : BaseStrategy
{
    private readonly int _fastPeriod;
    private readonly int _slowPeriod;

    public EmaCrossStrategy(int fast = 9, int slow = 21) : base("ema-cross")
    {
        _fastPeriod = fast;
        _slowPeriod = slow;
    }

    public override IReadOnlyList<Signal> Evaluate(IReadOnlyList<Candle> candles)
    {
        if (candles.Count < _slowPeriod + 1) return [];
        var fastNow = Ema(candles, _fastPeriod, 0);
        var fastPrev = Ema(candles, _fastPeriod, 1);
        var slowNow = Ema(candles, _slowPeriod, 0);
        var slowPrev = Ema(candles, _slowPeriod, 1);
        var last = candles[^1];

        if (fastPrev <= slowPrev && fastNow > slowNow)
            return [new Signal { Side = "buy", Price = last.Close, Quantity = 1, Reason = "ema cross up" }];
        if (fastPrev >= slowPrev && fastNow < slowNow)
            return [new Signal { Side = "sell", Price = last.Close, Quantity = 1, Reason = "ema cross down" }];
        return [];
    }

    private static decimal Ema(IReadOnlyList<Candle> candles, int period, int offset)
    {
        var start = candles.Count - period - offset;
        if (start < 0) start = 0;
        var k = 2m / (period + 1);
        var ema = candles[start].Close;
        for (var i = start + 1; i < candles.Count - offset; i++)
            ema = candles[i].Close * k + ema * (1 - k);
        return ema;
    }
}
