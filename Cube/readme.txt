CUBE – Travel Across a 3-D Cube
Version: C# Console Edition
Genre: Logic / Risk Management / Puzzle
Players: 1

Objective
Navigate from the starting point (1,1,1) to the goal (3,3,3) inside a virtual 3×3×3 cube without hitting hidden mines or making illegal moves.

Starting the Game
To play, double-click run.bat in the game’s folder. This will build and run the game, and keep the window open when it finishes.

Gameplay Overview
You begin at (1,1,1) with $500 in your account.

The computer secretly places 5 mines at random coordinates in the cube (never at start or goal).

You move one space at a time, changing only one coordinate by ±1 per move.

The goal is to reach (3,3,3) without hitting a mine.

Controls & Input
At the start of each round, you can choose to make a wager or skip betting.

To move, enter your new coordinates in one of the following formats:

Example: 2 1 1

Example: 2,1,1

Q or QUIT at a wager prompt exits the game.

Rules
Movement Restriction – You must change exactly one coordinate by 1 each turn. Example:

✅ Allowed: (1,1,1) → (2,1,1)
❌ Not allowed: (1,1,1) → (2,2,1) (two coordinates changed)

Mines – If you step on a mine, you instantly lose the round and your wager.

Illegal Moves – Entering coordinates outside 1..3 or changing multiple coordinates loses the round.

Winning a Round – Reach (3,3,3) to win your wager amount.

Winning & Losing
Win Round: Bankroll increases by your wager.

Lose Round: Bankroll decreases by your wager.

Game Over: Bankroll reaches $0, or you quit.

Tips
Picture the cube in layers:

Layer Z=1, Layer Z=2, Layer Z=3.

Move carefully — rushing increases the chance of stepping on a mine.

Avoid moving back and forth in loops; plan ahead for the shortest safe path.

