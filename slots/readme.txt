SLOTS — Slot Machine

A simulation of the classic casino slot machine, where you spin the reels and hope to line up a winning combination.

Objective

Start with a set amount of money and try to increase it by spinning the slot machine reels and hitting winning combinations.

Requirements

.NET 6 or newer installed (download here).

Runs in a standard console (Windows, macOS, or Linux).

No extra libraries needed — pure text output.

Game Setup

You start with a certain bankroll (the BASIC version assumes you insert a quarter each time you play; the C# version starts you with a set total).

The reels display three symbols each spin.

Possible symbols include:

BELL

BAR

CHERRY

APPLE

LEMON

$ (Jackpot symbol)

How to Play

The game will prompt you to spin:

AGAIN? (Y/N)


Press Y to pull the handle (spin) or N to quit.

Each spin randomly generates three symbols.

The program checks if you’ve hit a winning combination and awards the appropriate payout.

Payouts

Certain symbol matches award fixed amounts:

Keno (special match condition) — $5

Matching certain symbols — $1

Jackpot — $20 (default; can be adjusted)

You can lose your stake if no winning combination appears.

Winning & Losing

Your total bankroll changes after each spin.

The game ends when:

You choose N at the "AGAIN?" prompt.

You run out of money.

Example Gameplay
BELL  APPLE  BELL  YOU HAVE WON $1 --- TOTAL=$ 1  AGAIN? Y

APPLE  APPLE  CHERRY  KENO.. YOU WIN $5. TOTAL=$ 6  AGAIN? Y

APPLE  APPLE  APPLE  JACKPOT...$20. TOTAL=$ 26  AGAIN? Y

Strategy Notes

This is purely a game of chance — no skill involved.

In the BASIC version, the odds are player-friendly (+11% over neutral).

For a more realistic challenge, reduce jackpot to $15 and Keno to $4 in the code.
