ROULET — European Roulette (Single Zero)
📜 Overview

This is a console version of European Roulette (single zero), inspired by the BASIC game in 101 BASIC Computer Games.
You can place one or more types of bets on each spin:

Odd/Even — pays 1:1

Red/Black — pays 1:1

Column (1, 2, or 3) — pays 2:1

Single Number (0–36) — pays 35:1

The game simulates a spin of the wheel, evaluates your bets, and keeps a running total of your winnings or losses.

💻 Requirements

OS: Windows 10/11 (x64)

.NET:

Recommended: .NET 8 or 9 SDK (to build and run)

Minimum to run only: .NET 8 or 9 Runtime

Console Application — runs in a terminal (CMD, PowerShell, or similar)

No extra dependencies — everything is built into .NET

🛠 Installation
1. Install .NET

Download the .NET 8 SDK (or newer) from:
https://dotnet.microsoft.com/download

Verify installation:

dotnet --info


You should see .NET SDK version 8.x or 9.x.

2. Get the Game

If you have the full project already:

Double-click run.bat (if present), or

From the project folder:

dotnet run -c Release


If starting from the source file:

dotnet new console -n Roulette
move Program.cs Roulette\Program.cs
cd Roulette
dotnet run -c Release

🎮 How to Play
Step 1 — The Game’s Welcome Message

You’ll see:

Betting rules

Payoffs for each bet type

Min/Max bet limits

Step 2 — Placing Bets

For each round, the game asks in turn:

Odd/Even bet?

Answer YES or NO.

If yes: choose ODD or EVEN, then enter your bet amount.

Red/Black bet?

Answer YES or NO.

If yes: choose RED or BLACK, then enter your bet amount.

Column bet?

Answer YES or NO.

If yes: choose column number 1, 2, or 3, then enter your bet amount.

Single Number bet?

Answer YES or NO.

If yes: enter a number 0 through 36, then enter your bet amount.

💡 You can place any combination of these bets on the same spin.

Step 3 — Spin & Result

The game spins the wheel and prints:

THE NUMBER IS 15  RED, COLUMN 3


Each of your bets is evaluated:

Winning bets: payout printed with amount won

Losing bets: amount lost

Zero: only a direct bet on 0 wins; all others lose.

Step 4 — Round Summary

Shows net profit or loss for the round

Updates your cumulative total across all rounds

Step 5 — Play Again?

Answer YES to continue betting, NO to end the session.

When you quit, the game thanks you for playing.

📊 Payouts
Bet Type	Win Condition	Payout
Odd/Even	Number matches chosen parity (≠0)	1:1
Red/Black	Number matches chosen color (≠0)	1:1
Column	Number in chosen column (≠0)	2:1
Single Number	Exact match, including zero	35:1
💡 Tips for Players

Zero (green) is neither odd/even nor red/black, and belongs to no column — it’s the house edge.

You can bet on multiple categories to hedge risks.

Manage your bankroll — big bets on single numbers can pay huge, but have low odds.

🔧 Troubleshooting

Game won’t run:

Make sure you have the correct .NET Runtime/SDK installed.

Run from a terminal, not by double-clicking the .cs file.

Invalid input:

For YES/NO questions, type YES or NO.

For numeric bets, only whole dollar amounts between 1 and 10,000 are accepted.