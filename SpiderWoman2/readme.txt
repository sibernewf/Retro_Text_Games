Spiderwoman (Bonus Rule) — Player Documentation
Overview

In Spiderwoman (Bonus Rule), the computer secretly chooses a random letter (A–Z).
You must discover this letter by entering words. If the hidden letter is in your word, you can attempt to guess it. This version adds a bonus twist: if you choose to guess, you get up to two attempts. Fail both, however, and Spiderwoman punishes you by deducting five goes!

Installation & Running

Install the .NET 6 SDK or later.
Download here
.

Open a terminal or command prompt in the project folder.

Build and run with:

dotnet run


The game plays entirely in your terminal.

How to Play

At the start, Spiderwoman secretly selects a letter from A–Z.

Enter a word (must be 4–8 characters long).

If the hidden letter is not in your word:

'X' IS NOT IN THAT WORD


If the hidden letter is in your word:

YES – IT'S ONE OF THOSE
DO YOU WANT TO GUESS? (Y/N)


If you answer Y:

You get two guesses to name the hidden letter.

If you guess correctly:

OK – YOU CAN GO (THIS TIME)


…and you win.

If you guess wrong twice:

WRONG! YOU FORFEIT FIVE GOES.


…and 5 turns are added to your go counter.

If you answer N, play continues and you can try more words.

Rules

Each word you enter counts as one go .

If you exceed 15 goes (including penalties), you automatically lose:

YOU ARE TOO LATE
YOU ARE NOW A FLY


Word length must be 4–8 characters. Too short or too long words are rejected.

You may only win by guessing the letter correctly.

Strategy Tips

Use words that contain several common letters to narrow the possibilities.

Save your guesses for when you are confident, since a double failure costs 5 goes.

Keep track of your total goes; you only have 15 before Spiderwoman ends the game.

Ending

Win: Correctly guess the hidden letter within the limit.

Lose:

Wrong guesses (after the two attempts), or

Reaching more than 15 goes.
