using System.Text.Json;
using TradeBot.Engine.Core;
using TradeBot.Engine.Data;
using TradeBot.Engine.Exchange;
using TradeBot.Engine.Strategy;

namespace TradeBot.Bot;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        var configPath = args.Length > 1 ? args[1] : "appsettings.json";
        var config = BotConfig.LoadOrDefault(Path.Combine(AppContext.BaseDirectory, configPath));
        var client = new ExchangeClient("https://api.sim", config.ApiKey, config.ApiSecret);
        var store = new CandleStore();
        var risk = new RiskManager(config);
        var router = new OrderRouter(client, risk);
        var backtest = new BacktestRunner(store);

        if (args.Length == 0)
        {
            PrintHelp();
            return 0;
        }

        return args[0].ToLowerInvariant() switch
        {
            "backtest" => RunBacktest(config, backtest),
            "paper" => await RunPaper(config, client, store, risk, router),
            "status" => await ShowStatus(client, router, config),
            "config" => ShowConfig(config),
            "orders" => ShowOrders(client),
            _ => Unknown(args[0])
        };
    }

    private static int RunBacktest(BotConfig config, BacktestRunner bt)
    {
        BaseStrategy strat = config.Strategy switch
        {
            "grid" => new GridStrategy(35000, 25000, 10, 0.01m),
            "dca" => new DcaStrategy(100),
            "ema" or "ema-cross" => new EmaCrossStrategy(),
            _ => new GridStrategy(35000, 25000, 10, 0.01m)
        };
        var result = bt.Run(strat, config.Symbol, config.TimeFrame, config.BacktestDays);
        Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }

    private static async Task<int> RunPaper(BotConfig config, ExchangeClient client,
        CandleStore store, RiskManager risk, OrderRouter router)
    {
        Console.WriteLine($"[paper] starting on {config.Symbol}...");
        var candles = store.Get(config.Symbol, config.TimeFrame, 50);
        BaseStrategy strat = config.Strategy == "dca" ? new DcaStrategy(100) : new GridStrategy(35000, 25000, 10, 0.01m);
        var signals = strat.Evaluate(candles);
        foreach (var sig in signals.Take(3))
        {
            var price = await client.GetPriceAsync(config.Symbol);
            var id = await router.RouteSignalAsync(sig, config.InitialCapital);
            Console.WriteLine($"  {sig.Side} {sig.Quantity} @ {sig.Price} -> {id ?? "blocked"}");
        }
        Console.WriteLine($"[paper] pnl={router.TotalPnl:F2}");
        return 0;
    }

    private static async Task<int> ShowStatus(ExchangeClient client, OrderRouter router, BotConfig config)
    {
        var price = await client.GetPriceAsync(config.Symbol);
        Console.WriteLine($"symbol: {config.Symbol}");
        Console.WriteLine($"price:  {price}");
        Console.WriteLine($"mode:   {config.Mode}");
        Console.WriteLine($"trades: {router.TradeHistory.Count}");
        Console.WriteLine($"pnl:    {router.TotalPnl:F2}");
        return 0;
    }

    private static int ShowConfig(BotConfig config)
    {
        Console.WriteLine(JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }

    private static int ShowOrders(ExchangeClient client)
    {
        foreach (var o in client.AllOrders)
            Console.WriteLine($"{o.Id}  {o.Side}  {o.Quantity}  {o.Price}  {o.Status}");
        if (client.AllOrders.Count == 0) Console.WriteLine("(no orders)");
        return 0;
    }

    private static int Unknown(string cmd) { Console.Error.WriteLine($"unknown: {cmd}"); PrintHelp(); return 1; }
    private static void PrintHelp() => Console.WriteLine(@"tradebot — crypto trading bot (simulated)

  backtest   Run strategy backtest
  paper      Paper-trade session
  status     Bot status
  config     Show config
  orders     List orders
");
}
