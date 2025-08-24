SALVO I — Army Gun Battle

A head-to-head guessing game where you and the computer hide platoons and try to destroy each other’s positions.

Objective

Destroy all four of the enemy’s platoons before the enemy destroys all of yours.

Requirements

.NET 6 or newer installed (download here).

Runs on Windows, macOS, or Linux in a terminal.

No extra packages or graphics required — it’s a text-only console game.

To check if you have .NET installed:

dotnet --version


If the version is 6.0 or higher, you’re good to go.

Game Setup

The game uses a 5×5 battlefield grid, with outposts numbered 1 to 25:

 1   2   3   4   5
 6   7   8   9  10
11  12  13  14  15
16  17  18  19  20
21  22  23  24  25


You choose four unique numbers (1–25) where your platoons will be hidden.

The computer also hides four platoons on its own secret grid.

How to Play

Your Turn: Enter a number (1–25) to fire at that outpost on the enemy’s grid.

If it contains an enemy platoon, you’ll score a hit.

If it’s empty, it’s a miss.

Computer’s Turn: The computer randomly fires at one of your outposts.

You’ll be told if it hit or missed.

You cannot fire at the same location twice.

Winning the Game

You win by destroying all four of the enemy’s platoons before they destroy all of yours.

The game ends immediately once one side’s platoons are all gone.

Example Gameplay
WHAT ARE YOUR FOUR POSITIONS? 10 15 20 25

WHERE DO YOU WISH TO FIRE YOUR MISSILE? 6
HA, HA YOU MISSED.  MY TURN NOW
I MISSED YOU, YOU DIRTY RAT.  I PICKED 3.  YOUR TURN.

WHERE DO YOU WISH TO FIRE YOUR MISSILE? 15
I GOT YOU.  YOUR OUTPOST WAS HIT.
YOU HAVE ONLY THREE OUTPOSTS LEFT!

Tips & Strategy

Spread out your platoons to make them harder to find.

Track your shots to avoid wasting turns firing at the same place twice.

Remember that the enemy’s platoons can be anywhere from 1 to 25 — no patterns guaranteed.