The Number Wizard — Player Documentation
Overview

In The Number Wizard, you are locked in a magical duel.
Two dice are thrown each turn. You must choose two numbers (0–9) that add up to the dice total. Each number you choose becomes “used up” — except 0, which can be reused forever.

Your goal is to use up all the numbers 1–9 before your turns run out. If you succeed, you outwit the Wizard. If you fail, the Wizard wins.

Installation & Running

Ensure you have the .NET 6 SDK or later installed.
Download from Microsoft
.

Open a terminal/command prompt in the project folder.

Run:

dotnet run

How to Play

The game begins with all numbers 0–9 available.

Numbers 1–9 can only be used once.

Number 0 can be reused as often as you like
.

Each turn:

Two dice are rolled (shown on screen).

The dice total is the sum you must match.

You then enter two numbers (0–9) whose sum equals the dice total.

Example: If the dice show 4 and 5 (total = 9), you might enter 0 and 9, or 4 and 5.

If both numbers are valid and still available:

They are marked as used (removed from play).

If the number was 0, it remains reusable.

If you roll a double (both dice the same), you gain an extra turn
.

Each round normally costs 1 turn. You start with 8 turns.

Winning & Losing

Win Condition: You remove all numbers 1–9 from play before your turns run out.
You’ll see:

YOU WON


Lose Condition: Your turns run out before you clear the numbers.
You’ll see:

THE WIZARD WON

Rules Summary

Start with 8 turns.

Using a double dice roll gives you a bonus turn.

You must always pick two numbers that add to the dice total.

Numbers 1–9 can only be used once.

Number 0 can be used any number of times.

If you enter invalid numbers (too big, already used, or don’t sum correctly), the turn is wasted.

Strategy Tips

Save 0 for tricky dice totals — it’s the only reusable number.

Think ahead: avoid combinations that will leave impossible sums later.

Remember that doubles help you — try to take advantage of the bonus turns.
