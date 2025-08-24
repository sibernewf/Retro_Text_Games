CHOMP – Player Documentation
Note: To play, double-click run.bat in the game’s folder. This will build and run the game, and keep the window open when it finishes.

Overview
CHOMP is a turn-based multiplayer game where players take turns “eating” sections of a rectangular cookie. The top-left square is poisoned. If you’re forced to eat it, you lose instantly.

The computer acts only as the moderator. Any number of human players can participate (2–9).

Objective
Avoid being the player who is forced to eat the poison square P at the top-left corner.

Setup
Launch the game by double-clicking run.bat.

Rules Prompt – You’ll be asked:

Want the rules (Y/N)?

Press Y to see the full rules.

Press N to skip directly to setup.

Players – Enter how many people are playing (2–9).

Board Size – Enter the number of rows (1–9) and columns (1–9).

Board Display
The board is shown with:

Numbers along the top and bottom for columns.

Numbers on the left for rows.

P marks the poison square.

* marks edible squares.

Blank spaces are already-eaten squares.

Example:

   1 2 3 4
  +-------+
1 |P * * *|
2 |* * * *|
3 |* * * *|
  +-------+
   1 2 3 4
How to Play
On your turn, you’ll see:

PLAYER X
COORDINATES OF CHOMP (ROW,COLUMN)?
Type the row and column of the square you want to eat:

You can use either a comma (3,4) or a space (3 4) between numbers.

All squares at or below your chosen row, and to the right of your chosen column (including the chosen square) will be eaten.

Play continues clockwise to the next player.

Special Rules
Poison Square: If you chomp P (top-left), you instantly lose.

Illegal Moves:

Trying to chomp an empty square:

"NO FAIR. YOU'RE TRYING TO CHOMP ON EMPTY SPACE!"

Choosing a square outside the original board:

"NO FAIR. THAT'S OUTSIDE THE ORIGINAL DIMENSIONS OF THE COOKIE."

In both cases, you repeat your turn.

Streak Counter:

The player who wins gets +1 to their win streak.

The player who loses to poison has their streak reset to 0.

Streaks persist across multiple games in the same session.

Winning
If you force the next player to be stuck with the poison square, you win.

The loser is the one who chomped P.

Playing Again
After each game, you’ll be asked:

AGAIN (Y/N)?
Y – Play another round with the same number of players (streaks are kept).

N – End the game.