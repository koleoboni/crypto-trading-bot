using TradeBot.Engine.Strategy;
using TradeBot.Engine.Data;
using Xunit;

namespace TradeBot.Engine.Tests;

public class GridTests
{
    [Fact]
    public void Grid_levels_evenly_spaced()
    {
        var grid = new GridStrategy(40000, 30000, 6, 0.01m);
        var levels = grid.GridLevels();
        Assert.Equal(6, levels.Count);
        Assert.Equal(30000m, levels[0]);
        Assert.Equal(40000m, levels[^1]);
    }

    [Fact]
    public void Grid_evaluate_returns_signals()
    {
        var grid = new GridStrategy(35000, 25000, 5, 0.01m);
        var candles = new List<Candle>
        {
            new() { Close = 30000, Open = 30000, High = 30100, Low = 29900, Volume = 100 }
        };
        var signals = grid.Evaluate(candles);
        Assert.NotNull(signals);
    }
}

public class DcaTests
{
    [Fact]
    public void Dca_increments_buy_count()
    {
        var dca = new DcaStrategy(100);
        var candles = new List<Candle>
        {
            new() { Close = 30000, Open = 30000, High = 30100, Low = 29900, Volume = 100 },
            new() { Close = 29500, Open = 30000, High = 30000, Low = 29400, Volume = 120 }
        };
        var signals = dca.Evaluate(candles);
        Assert.Single(signals);
        Assert.Equal("buy", signals[0].Side);
        Assert.Equal(1, dca.BuyCount);
    }
}

public class MetricsTests
{
    [Fact]
    public void MaxDrawdown_correct()
    {
        var curve = new List<decimal> { 100, 110, 105, 120, 90 };
        var dd = MetricsCalc.MaxDrawdown(curve);
        Assert.True(dd > 0);
    }

    [Fact]
    public void ProfitFactor_positive()
    {
        var returns = new List<decimal> { 10, -5, 20, -8, 15 };
        Assert.True(MetricsCalc.ProfitFactor(returns) > 1);
    }
}

public class BacktestTests
{
    [Fact]
    public void Backtest_runs_grid()
    {
        var store = new CandleStore();
        var runner = new BacktestRunner(store);
        var result = runner.Run(new GridStrategy(35000, 25000, 5, 0.01m), "BTCUSDT", "1h", 7);
        Assert.Equal("grid", result.Strategy);
    }
}
