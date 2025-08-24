1CHECK — Solitaire Checker Game
Player Documentation

Objective
Remove as many checkers as possible from the board by making diagonal jumps (similar to standard checkers). The game starts with 48 checkers placed on the outer two rings of an 8×8 checkerboard.

Your goal is to maximize the number of jumps and minimize the number of checkers remaining.

30–39 removed: Easy

40–44 removed: Challenging

45–47 removed: Exceptional

How to Play
Board Layout

The board is numbered 1 to 64, starting from the top-left to bottom-right.

At the start, the outer two rings are filled with checkers, leaving the inner 4×4 empty.

Moves

A legal move is a single diagonal jump:

Move from a square containing a checker

Jump over an adjacent checker

Land in an empty square exactly two spaces away diagonally

The jumped checker is removed from the board

You cannot:

Jump vertically or horizontally

Jump over an empty square

Jump more than one piece in a single move

After every legal move:

The board is updated on screen

Your move is logged to a game log file

Commands During Play

FROM TO — Enter two numbers (1..64) to make a move.
Example: 12 26

LIST — Show all legal jumps available from the current position.

HELP — Show the numbered reference board and rules.

QUIT — End the game early.

[Enter] — Skip input if you need more thinking time.

Game End

The game ends automatically when no legal moves are available.

A summary is shown:

Total jumps made

Pieces remaining on the board

A log file is saved with:

Every move made

Board states after each move

Final summary

Logging

Log files are named:
1check-log-YYYYMMDD-HHMMSS.txt

They are stored in the same directory as the executable.

The log includes:

Game start time

Every move in sequence

Board state snapshots in 1 (checker) / 0 (empty) format

Final game stats