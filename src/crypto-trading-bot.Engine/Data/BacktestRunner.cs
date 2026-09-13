using TradeBot.Engine.Strategy;

namespace TradeBot.Engine.Data;

public sealed class BacktestRunner
{
    private readonly CandleStore _store;

    public BacktestRunner(CandleStore store) => _store = store;

    public BacktestResult Run(BaseStrategy strategy, string symbol, string timeframe, int days)
    {
        var candles = _store.Get(symbol, timeframe, days * 24);
        if (candles.Count < 2)
            return new BacktestResult { Strategy = strategy.Name, Symbol = symbol };

        var equity = 10000m;
        var peak = equity;
        var maxDrawdown = 0m;
        var trades = 0;
        var wins = 0;
        var totalPnl = 0m;

        strategy.Reset();

        for (var i = 2; i <= candles.Count; i++)
        {
            var window = candles.Take(i).ToList();
            var signals = strategy.Evaluate(window);
            foreach (var signal in signals)
            {
                var pnl = signal.Side == "buy"
                    ? (candles[Math.Min(i, candles.Count - 1)].Close - signal.Price) * signal.Quantity
                    : (signal.Price - candles[Math.Min(i, candles.Count - 1)].Close) * signal.Quantity;
                equity += pnl;
                totalPnl += pnl;
                trades++;
                if (pnl > 0) wins++;
            }

            if (equity > peak) peak = equity;
            var dd = peak > 0 ? (peak - equity) / peak : 0;
            if (dd > maxDrawdown) maxDrawdown = dd;
        }

        return new BacktestResult
        {
            Strategy = strategy.Name,
            Symbol = symbol,
            TotalTrades = trades,
            WinRate = trades > 0 ? (decimal)wins / trades * 100 : 0,
            TotalPnl = Math.Round(totalPnl, 2),
            MaxDrawdownPct = Math.Round(maxDrawdown * 100, 2),
            FinalEquity = Math.Round(equity, 2),
            SharpeApprox = trades > 0 ? Math.Round(totalPnl / (Math.Abs(totalPnl) + 1) * 2, 3) : 0
        };
    }
}

public sealed class BacktestResult
{
    public string Strategy { get; init; } = "";
    public string Symbol { get; init; } = "";
    public int TotalTrades { get; init; }
    public decimal WinRate { get; init; }
    public decimal TotalPnl { get; init; }
    public decimal MaxDrawdownPct { get; init; }
    public decimal FinalEquity { get; init; }
    public decimal SharpeApprox { get; init; }
}
