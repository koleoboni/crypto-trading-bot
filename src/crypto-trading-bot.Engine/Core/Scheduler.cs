namespace TradeBot.Engine.Core;

public sealed class Scheduler
{
    private readonly List<(string Name, TimeSpan Interval, Func<CancellationToken, Task> Action)> _tasks = [];
    private CancellationTokenSource? _cts;

    public void Register(string name, TimeSpan interval, Func<CancellationToken, Task> action)
    {
        _tasks.Add((name, interval, action));
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var runners = _tasks.Select(t => RunLoop(t.Name, t.Interval, t.Action, _cts.Token));
        await Task.WhenAll(runners);
    }

    private static async Task RunLoop(string name, TimeSpan interval, Func<CancellationToken, Task> action, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await action(ct);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{name}] error: {ex.Message}");
            }
            try { await Task.Delay(interval, ct); }
            catch (OperationCanceledException) { break; }
        }
    }

    public void Stop() => _cts?.Cancel();
}
