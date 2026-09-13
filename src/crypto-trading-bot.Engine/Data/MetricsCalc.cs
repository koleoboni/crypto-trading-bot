namespace TradeBot.Engine.Data;

public static class MetricsCalc
{
    public static decimal Sharpe(IReadOnlyList<decimal> returns, decimal riskFreeRate = 0)
    {
        if (returns.Count < 2) return 0;
        var avg = returns.Average();
        var variance = returns.Sum(r => (r - avg) * (r - avg)) / (returns.Count - 1);
        var stdDev = (decimal)Math.Sqrt((double)variance);
        return stdDev == 0 ? 0 : Math.Round((avg - riskFreeRate) / stdDev, 4);
    }

    public static decimal Sortino(IReadOnlyList<decimal> returns, decimal riskFreeRate = 0)
    {
        if (returns.Count < 2) return 0;
        var avg = returns.Average();
        var downside = returns.Where(r => r < riskFreeRate).ToList();
        if (downside.Count == 0) return avg > 0 ? 99m : 0;
        var downVar = downside.Sum(r => (r - riskFreeRate) * (r - riskFreeRate)) / downside.Count;
        var downDev = (decimal)Math.Sqrt((double)downVar);
        return downDev == 0 ? 0 : Math.Round((avg - riskFreeRate) / downDev, 4);
    }

    public static decimal MaxDrawdown(IReadOnlyList<decimal> equityCurve)
    {
        if (equityCurve.Count == 0) return 0;
        var peak = equityCurve[0];
        var maxDd = 0m;
        foreach (var eq in equityCurve)
        {
            if (eq > peak) peak = eq;
            var dd = peak > 0 ? (peak - eq) / peak : 0;
            if (dd > maxDd) maxDd = dd;
        }
        return Math.Round(maxDd * 100, 2);
    }

    public static decimal ProfitFactor(IReadOnlyList<decimal> returns)
    {
        var gross = returns.Where(r => r > 0).Sum();
        var loss = Math.Abs(returns.Where(r => r < 0).Sum());
        return loss == 0 ? (gross > 0 ? 99m : 0) : Math.Round(gross / loss, 3);
    }
}
