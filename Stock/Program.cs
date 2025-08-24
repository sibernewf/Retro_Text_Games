using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace StockMarketGame
{
    internal static class Program
    {
        // Money handling—use decimal for currency
        private const decimal StartingCash = 10_000m;
        private const decimal BrokerageFeeRate = 0.01m; // 1% per transaction

        private static readonly CultureInfo Us = CultureInfo.GetCultureInfo("en-US");

        private static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("THE STOCK MARKET");
            Console.WriteLine();
            if (AskYesNo("DO YOU WANT THE INSTRUCTIONS (YES=TYPE 1, NO=TYPE 0)? "))
            {
                PrintInstructions();
            }

            var rand = new Random();

            // Five classic tickers used in the magazine example.
            var stocks = new List<Stock>
            {
                new("IBM", 100m),
                new("RCA",  85m),
                new("LGB", 120m),  // “LGB/ABC” varies across old listings—use LGB here
                new("ABC",  90m),
                new("CBS",  95m)
            };

            // Each stock gets a trend with a slope and a duration (days remaining)
            foreach (var s in stocks)
                s.ResetTrend(rand);

            var portfolio = new Portfolio(cash: StartingCash);

            int day = 0;
            while (true)
            {
                day++;
                Console.WriteLine();
                Console.WriteLine(new string('*', 12) + "  END OF DAY'S TRADING  " + new string('*', 12));
                Console.WriteLine();

                // 1) Print table
                PrintTable(stocks, portfolio);

                // 2) Ask for transactions per symbol (in shares)
                Console.WriteLine();
                Console.WriteLine("WHAT IS YOUR TRANSACTION IN");
                foreach (var s in stocks)
                {
                    var qty = AskInt($"{s.Symbol}? ", allowBlank: true);
                    if (qty == 0) continue;

                    // Price * shares ± fee; validate funds/holdings
                    var gross = (decimal)qty * s.Price;
                    var fee   = Math.Abs(gross) * BrokerageFeeRate;
                    var netCashChange = -(gross + fee); // positive qty reduces cash

                    if (qty > 0)
                    {
                        if (portfolio.Cash + netCashChange < 0)
                        {
                            Console.WriteLine("YOU HAVE USED MORE THAN YOU HAVE.  TRY AGAIN.");
                            qty = 0;
                            continue;
                        }
                        portfolio.Cash += netCashChange;
                        portfolio.Add(s.Symbol, qty);
                    }
                    else
                    {
                        // Selling
                        var have = portfolio.GetShares(s.Symbol);
                        if (-qty > have)
                        {
                            Console.WriteLine("YOU CANNOT SELL MORE SHARES THAN YOU OWN.  TRY AGAIN.");
                            qty = 0;
                            continue;
                        }
                        portfolio.Add(s.Symbol, qty); // qty is negative
                        portfolio.Cash += -(gross) - fee; // gross is negative; add proceeds less fee
                    }
                }

                // 3) Advance one day: update prices via trend, maybe flip trends
                var oldIndex = IndexLevel(stocks);
                foreach (var s in stocks) s.Step(rand);
                var newIndex = IndexLevel(stocks);

                // 4) Print end-of-day summary
                Console.WriteLine();
                PrintTable(stocks, portfolio);

                Console.WriteLine($"NEW YORK STOCK EXCHANGE AVERAGE: {newIndex.ToString("0.##", Us)}   NET CHANGE: {(newIndex - oldIndex).ToString("+0.##;-0.##;0", Us)}");
                Console.WriteLine();

                var totalStockAssets = portfolio.TotalStockValue(stocks);
                Console.WriteLine($"TOTAL STOCK ASSETS ARE   $ {totalStockAssets.ToString("0.##", Us)}");
                Console.WriteLine($"TOTAL CASH ASSETS ARE    $ {portfolio.Cash.ToString("0.##", Us)}");
                Console.WriteLine($"TOTAL ASSETS ARE         $ {(totalStockAssets + portfolio.Cash).ToString("0.##", Us)}");

                if (!AskYesNo("DO YOU WISH TO CONTINUE (YES=TYPE 1, NO=TYPE 0)? "))
                    break;
            }

            Console.WriteLine();
            Console.WriteLine("HOPE YOU HAD FUN!!");
        }

        private static void PrintInstructions()
        {
            Console.WriteLine();
            Console.WriteLine("THIS PROGRAM PLAYS THE STOCK MARKET.  YOU WILL BE GIVEN $10,000");
            Console.WriteLine("AND MAY BUY OR SELL STOCKS.  THE STOCK PRICES WILL");
            Console.WriteLine("BE RANDOMLY AFFECTED BY 'TRENDS' ON THE EXCHANGE, SO THESE");
            Console.WriteLine("DO NOT REPRESENT EXACTLY WHAT HAPPENS ON THE EXCHANGE.");
            Console.WriteLine("A 1% BROKERAGE FEE IS CHARGED ON ALL TRANSACTIONS.");
            Console.WriteLine("ENTER THE NUMBER OF SHARES: POSITIVE TO BUY, NEGATIVE TO SELL,");
            Console.WriteLine("OR 0 TO DO NOTHING.  EVEN IF A STOCK DROPS TO ZERO IT MAY REBOUND.");
            Console.WriteLine("TRY TO PLAY FOR AT LEAST 10 DAYS TO GET A FEEL FOR THE MARKET.");
            Console.WriteLine("GOOD LUCK!");
            Console.WriteLine();
        }

        private static void PrintTable(List<Stock> stocks, Portfolio portfolio)
        {
            Console.WriteLine("{0,-6} {1,12} {2,10} {3,12} {4,16}",
                "STOCK", "PRICE/SHARE", "HOLDINGS", "VALUE", "NET PRICE CHANGE");
            foreach (var s in stocks)
            {
                var shares = portfolio.GetShares(s.Symbol);
                var value  = shares * s.Price;
                var delta  = s.LastNetChange;
                Console.WriteLine("{0,-6} {1,12} {2,10} {3,12} {4,16}",
                    s.Symbol,
                    s.Price.ToString("0.##", Us),
                    shares,
                    value.ToString("0.##", Us),
                    delta.ToString("+0.##;-0.##;0", Us));
            }
            Console.WriteLine();
        }

        private static bool AskYesNo(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine();
                if (s == null) continue;
                s = s.Trim();
                if (s == "1" || s.Equals("yes", StringComparison.OrdinalIgnoreCase)) return true;
                if (s == "0" || s.Equals("no",  StringComparison.OrdinalIgnoreCase)) return false;
            }
        }

        private static int AskInt(string prompt, bool allowBlank = false)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(s))
                    return allowBlank ? 0 : 0;
                if (int.TryParse(s, NumberStyles.Integer, Us, out var v))
                    return v;
                Console.WriteLine("PLEASE ENTER A WHOLE NUMBER (E.G., -10, 0, 25).");
            }
        }

        private static decimal IndexLevel(IEnumerable<Stock> stocks)
        {
            // Simple average of prices (to mimic the listing's "NY STOCK EXCHANGE AVERAGE")
            var arr = stocks.ToArray();
            return arr.Length == 0 ? 0 : arr.Sum(s => s.Price) / arr.Length;
        }
    }

    internal sealed class Portfolio
    {
        public decimal Cash { get; set; }
        private readonly Dictionary<string, int> _holdings = new(StringComparer.OrdinalIgnoreCase);

        public Portfolio(decimal cash)
        {
            Cash = cash;
        }

        public int GetShares(string symbol) => _holdings.TryGetValue(symbol, out var n) ? n : 0;

        public void Add(string symbol, int deltaShares)
        {
            _holdings.TryGetValue(symbol, out var cur);
            cur += deltaShares;
            if (cur == 0) _holdings.Remove(symbol);
            else _holdings[symbol] = cur;
        }

        public decimal TotalStockValue(IEnumerable<Stock> stocks)
        {
            decimal total = 0m;
            foreach (var s in stocks)
                total += GetShares(s.Symbol) * s.Price;
            return total;
        }
    }

    internal sealed class Stock
    {
        public string Symbol { get; }
        public decimal Price { get; private set; }

        // Trend model
        private int _trendSign;               // +1 or -1
        private decimal _trendSlope;          // % max magnitude per day (e.g., 0.05m = 5%)
        private int _trendDaysRemaining;      // days left before a recalculation

        public decimal LastNetChange { get; private set; } // absolute price delta last step

        public Stock(string symbol, decimal initialPrice)
        {
            Symbol = symbol;
            Price = initialPrice;
        }

        public void ResetTrend(Random rand)
        {
            _trendSign = rand.Next(0, 2) == 0 ? -1 : 1;
            // Slope between 1% and 6%
            _trendSlope = 0.01m + (decimal)rand.NextDouble() * 0.05m;
            // Duration between 4 and 12 trading days
            _trendDaysRemaining = rand.Next(4, 13);
        }

        public void Step(Random rand)
        {
            // Randomized move around the trend slope
            // Draw a % change in [-slope, +slope] but biased toward the trend sign.
            var basePct = (decimal)rand.NextDouble() * _trendSlope;
            var pctChange = _trendSign * basePct;                  // move with the trend
            // add small noise
            pctChange += ((decimal)rand.NextDouble() - 0.5m) * 0.01m; // ±0.5%

            var oldPrice = Price;
            Price = Math.Max(0m, Price * (1m + pctChange)); // no negative price
            LastNetChange = Price - oldPrice;

            _trendDaysRemaining--;
            if (_trendDaysRemaining <= 0)
            {
                // Sometimes flip, sometimes keep same direction, always refresh slope/duration
                if (rand.NextDouble() < 0.35) _trendSign *= -1;
                _trendSlope = 0.01m + (decimal)rand.NextDouble() * 0.05m;
                _trendDaysRemaining = rand.Next(4, 13);
            }
        }
    }
}
