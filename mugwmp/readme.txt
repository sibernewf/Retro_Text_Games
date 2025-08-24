MUGWMP – FIND 4 MUGWUMPS IN HIDING

A deduction and triangulation challenge on a 10×10 grid.

📜 Objective

Your goal is to find the four Mugwumps hiding somewhere on a 10 by 10 coordinate grid.

The grid coordinates run:

X axis (horizontal): 0 (left) to 9 (right)

Y axis (vertical): 0 (bottom) to 9 (top)

The homebase is the bottom-left corner (0,0).

🗺 The Setup

The computer randomly hides 4 Mugwumps, each on a different square.

You cannot see them — your only clues are distances.

You have 10 turns to find them all.

🎲 How to Play

Enter your guess as a coordinate pair:

X,Y


Example:

5,7


This means 5 units right and 7 units up from homebase.

After each guess:

If your guess matches a Mugwump’s exact location, you find that Mugwump.

If not, the computer tells you the straight-line distance from your guess to each Mugwump still hiding.

Use distances from multiple turns to triangulate their positions.

Distances are shown with one decimal place.

⏱ Turns & Winning

You have 10 turns total.

The game ends early if you find all 4 Mugwumps.

If you run out of turns, the computer reveals all hiding spots.

💡 Tips for Success

Use graph paper to plot guesses and distances.

After two guesses, you can draw possible circles for each Mugwump — where those circles overlap is the hiding spot.

This is similar to LORAN or GPS triangulation.

📚 Commands

During the game prompt:

X,Y – Make a guess (both numbers 0–9).

HELP – Shows input instructions.

QUIT – End the game immediately and reveal all Mugwumps.

🏆 Scoring Ideas (optional if you want to track)

Perfect Score: Find all 4 in ≤ 6 turns.

Good: All found in ≤ 8 turns.

Okay: All found in ≤ 10 turns.

Try Again: Did not find all before time ran out.

📍 Example Game
MUGWMP — FIND 4 MUGWUMPS IN HIDING

TURN NO. 1  WHAT IS YOUR GUESS? 5,5
YOU ARE 4.2 UNITS FROM MUGWUMP 1
YOU ARE 6.4 UNITS FROM MUGWUMP 2
YOU ARE 3.6 UNITS FROM MUGWUMP 3
YOU ARE 5.1 UNITS FROM MUGWUMP 4

TURN NO. 2  WHAT IS YOUR GUESS? 3,4
YOU HAVE FOUND MUGWUMP 3
YOU ARE 5.0 UNITS FROM MUGWUMP 1
YOU ARE 4.5 UNITS FROM MUGWUMP 2
YOU ARE 2.2 UNITS FROM MUGWUMP 4

...
YOU GOT THEM ALL IN 7 TURNS!
