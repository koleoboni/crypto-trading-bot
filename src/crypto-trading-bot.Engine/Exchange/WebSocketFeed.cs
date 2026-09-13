using System.Security.Cryptography;
using System.Text;
using TradeBot.Engine.Strategy;

namespace TradeBot.Engine.Exchange;

public sealed class WebSocketFeed
{
    private readonly string _symbol;
    private readonly List<Candle> _buffer = [];
    private bool _running;

    public WebSocketFeed(string symbol)
    {
        _symbol = symbol;
    }

    public async Task StartAsync(Action<Candle> onCandle, CancellationToken ct = default)
    {
        _running = true;
        var basePrice = 30000m;
        var idx = 0;

        while (_running && !ct.IsCancellationRequested)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{_symbol}:{idx}"));
            var delta = (hash[0] - 128) / 100m;
            basePrice += delta * 10;
            var candle = new Candle
            {
                Time = DateTimeOffset.UtcNow,
                Open = basePrice,
                High = basePrice + Math.Abs(delta) * 5,
                Low = basePrice - Math.Abs(delta) * 5,
                Close = basePrice + delta * 3,
                Volume = (hash[1] + 1) * 100m
            };

            _buffer.Add(candle);
            if (_buffer.Count > 500) _buffer.RemoveAt(0);
            onCandle(candle);
            idx++;

            try { await Task.Delay(1000, ct); }
            catch (OperationCanceledException) { break; }
        }
    }

    public void Stop() => _running = false;

    public IReadOnlyList<Candle> Buffer => _buffer;
}
