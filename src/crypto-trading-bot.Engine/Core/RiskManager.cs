namespace TradeBot.Engine.Core;

public sealed class RiskManager
{
    private readonly BotConfig _config;
    private decimal _dailyPnl;
    private readonly object _lock = new();

    public RiskManager(BotConfig config)
    {
        _config = config;
        _dailyPnl = 0;
    }

    public bool CanOpenPosition(decimal positionValue, decimal portfolioValue)
    {
        if (portfolioValue <= 0) return false;
        var positionPct = positionValue / portfolioValue;
        if (positionPct > _config.MaxPositionPct) return false;
        lock (_lock)
        {
            if (_dailyPnl < -(_config.MaxDailyLossPct * portfolioValue))
                return false;
        }
        return true;
    }

    public void RecordPnl(decimal realized)
    {
        lock (_lock) { _dailyPnl += realized; }
    }

    public void ResetDaily()
    {
        lock (_lock) { _dailyPnl = 0; }
    }

    public decimal DailyPnl { get { lock (_lock) return _dailyPnl; } }
    public bool IsKillSwitchActive => DailyPnl < -(_config.MaxDailyLossPct * _config.InitialCapital);
}
