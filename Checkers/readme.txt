CHECKER – Player Documentation
Version: C# Console Edition with Move Logging & Multi-Jump Support
Original Author: Alan J. Segal
C# Conversion & Enhancements: 2025 Edition

Overview
This is a digital game of checkers where you play against the computer.

The computer's pieces are marked with X (Kings are K).

Your pieces are marked with 0 (Kings are Q).

The board uses coordinates where (1,1) is the bottom-left corner.

You take turns moving, with the computer moving first.

All mandatory captures (single or multi-jump) are enforced.

The game records every move and board state into a sample run text file for review later.

Objective
Capture all of your opponent’s pieces or block them so they have no legal moves.

How to Play
Start the Program
Run the game. You will see an introductory message explaining the rules and coordinate system.

Understand the Board Layout

The board is displayed from top row down to bottom row.

Pieces are only placed on dark squares (checkerboard pattern).

Coordinate (X,Y):

X = column number from left (1–8)

Y = row number from bottom (1–8)

Turns

The computer moves first, showing its move and the updated board.

Then it’s your turn. You will be prompted:

FROM x y TO u v:
Enter the coordinates of the piece you want to move and the square you want it to move to.
Example:

2 3 3 4
moves the piece at (2,3) to (3,4).

Captures

If a capture is available, you must take it.

Multiple jumps (double/triple captures) are allowed and required if possible.

Kings can move and capture in all diagonal directions.

Promotion

A piece reaching the opponent’s back row becomes a King (K for computer, Q for you).

Kings can move and capture backward as well as forward.

Winning and Losing

You win if the computer has no legal moves.

The computer wins if you have no legal moves.

Move Logging
Every game automatically creates a log file named:

Checkers_SampleRun_YYYYMMDD_HHMMSS.txt
This file contains:

Opening message and rules

Every move made by both players

Full board layout after every move

Input history for your moves

The final outcome (win/lose)

Example excerpt from a log:

I MOVE FROM (4 8) TO (3 7)
BOARD:
. X . X . X . X
X . X . X . X .
. X . X . X . X
. . . . . . . .
. . . . . . . .
0 . 0 . 0 . 0 .
. 0 . 0 . 0 . 0
0 . 0 . 0 . 0 .

YOUR MOVE:
FROM x y TO u v: 2 3 3 4
INPUT: 2 3 3 4
MOVE FROM (2 3) TO (3 4)
BOARD:
. X . X . X . X
X . X . X . X .
. X . X . X . X
. . . . . . . .
. 0 . . . . . .
0 . 0 . 0 . 0 .
. 0 . 0 . 0 . 0
0 . 0 . 0 . 0 .
Tips
Watch for forced captures — the game won’t let you skip them.

Multi-jumps can quickly swing the game in your favor.

Remember Kings have much more mobility — protect yours, and try to crown your pieces.

Review your sample run text file after the game to study moves and strategies.

To Play
Double-click run.bat in the game’s folder. This will build and run the game, keep the window open when it finishes, and automatically create a sample run log in the same folder.
