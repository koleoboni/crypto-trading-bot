# crypto-trading-bot

> ccxt · strategy · paper

Multi-exchange trading bot — strategy plugins, paper/live modes, CCXT-style adapters.

The default GitHub query when someone wants any crypto bot.

---

## Features

- Multi-exchange adapter layer (Binance, Bybit, OKX stubs)
- Strategy plugins: Grid, DCA, EMA Cross — or write your own
- Paper trading mode with full order simulation
- Backtesting engine with OHLCV data, slippage model
- Risk manager: max position %, daily loss kill switch
- Real-time websocket price feed (simulated)
- Sharpe, Sortino, drawdown, profit factor metrics

## Quick start

```bash
dotnet restore crypto-trading-bot.slnx
dotnet build crypto-trading-bot.slnx -c Release
dotnet run --project src/crypto-trading-bot.Bot -- backtest
```

## CLI

| Command | Description |
|---------|-------------|
| `backtest` | Run strategy backtest on historical OHLCV |
| `paper` | Start paper-trading session |
| `status` | Show bot status and PnL |
| `config` | Print current config |
| `orders` | List simulated orders |

## Configuration

Edit `src/crypto-trading-bot.Bot/appsettings.json`:

```json
{
  "Exchange": "binance",
  "Symbol": "BTCUSDT",
  "Mode": "paper",
  "Strategy": "grid",
  "TimeFrame": "1h"
}
```

## Strategies

| Strategy | Description |
|----------|-------------|
| `grid` | Buy/sell at evenly spaced price levels |
| `dca` | Fixed-interval buys with dip multiplier |
| `ema-cross` | EMA 9/21 crossover signals |

## Project structure

```
crypto-trading-bot/
├── src/
│   ├── crypto-trading-bot.Engine/
│   │   ├── Strategy/      # BaseStrategy, Grid, DCA, EMA
│   │   ├── Exchange/       # REST client, order router, WS feed
│   │   ├── Data/           # candle store, backtester, metrics
│   │   └── Core/           # config, risk manager, scheduler
│   └── crypto-trading-bot.Bot/         # CLI entry point
└── tests/
    └── crypto-trading-bot.Engine.Tests/
```

## Disclaimer

Simulated exchange I/O only. No real API keys, no real trades, no real funds at risk in the default build.

## License

MIT — Copyright (c) 2026


---

## Topics

![trading-bot](https://img.shields.io/badge/trading%20bot-111827?style=flat-square) ![crypto](https://img.shields.io/badge/crypto-111827?style=flat-square) ![bitcoin](https://img.shields.io/badge/bitcoin-111827?style=flat-square) ![ethereum](https://img.shields.io/badge/ethereum-111827?style=flat-square) ![algorithmic-trading](https://img.shields.io/badge/algorithmic%20trading-111827?style=flat-square) ![ccxt](https://img.shields.io/badge/ccxt-111827?style=flat-square) ![binance](https://img.shields.io/badge/binance-111827?style=flat-square) ![bybit](https://img.shields.io/badge/bybit-111827?style=flat-square)

`trading-bot` `crypto` `bitcoin` `ethereum` `algorithmic-trading` `ccxt` `binance` `bybit` `backtesting` `paper-trading` `cryptocurrency` `bot` `automated-trading` `dotnet` `csharp`

Search: crypto-trading-bot · ccxt · strategy · paper · Crypto trading bot — multi-exchange, strategy plugins, paper trading, backtesting

---

<sub>Crypto trading bot — multi-exchange, strategy plugins, paper trading, backtesting</sub>
