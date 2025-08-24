CHEMST – Dilute Kryptocyanic Acid Safely!
Player Documentation

Purpose
The goal of the game is to safely dilute Kryptocyanic Acid using exactly 7 parts water to 3 parts acid. Incorrect ratios cause a dangerous reaction, costing you a life.

Starting Instructions
Launch the game
Double-click run.bat in the game’s folder.
This will build and run the game, keeping the window open when it finishes.

Enter your name
This name will be displayed in messages and in the log file.

Practice Mode

When prompted, type Y to see the ideal answer after each round.

Type N to play without hints.

Understand the Ratio

Water needed = Acid Liters × (7 ÷ 3).

Example: 30 liters acid → 30 × (7/3) = 70 liters water.

More than ±5% off from the ideal amount costs you a life.

Gameplay Loop
You start with 9 lives and a streak counter (number of correct answers in a row).

Each round:

You’re given a random amount of acid (10–100 liters).

Type the amount of water needed (in liters).

The game checks your answer:

Correct (within ±5%) → You survive and your streak increases.

Wrong (more than ±5% off) → You lose a life and your streak resets.

The best streak achieved is tracked throughout the game.

Scoring & Streaks
Streak = Number of correct answers in a row.

Best streak = Highest streak reached during the game session.

Streak resets to 0 after a wrong answer.

Game Over
The game ends when all 9 lives are lost.

Your best streak is displayed.

Practice Mode Details
If ON, each round shows:

Ideal answer

Your answer

Error amount (liters)

Allowed tolerance (liters)

Log File
Every run creates a file named:
CHEMST_SampleRun_YYYYMMDD_HHMMSS.txt

The file contains all prompts, answers, and results from your session.

