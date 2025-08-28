Spiderwoman — Player Documentation
Overview

In Spiderwoman, the computer secretly chooses a random letter (A–Z).
Your task is to discover this letter by entering words. If the hidden letter is inside your word, you may attempt to guess it. Guess correctly and you escape; fail, and Spiderwoman turns you into a fly!

Installation & Running

Install the .NET 6 SDK or newer.
Download from Microsoft
.

Place the Program.cs file into a new console project (or open the provided project).
Example:

dotnet new console -n SpiderwomanGame
cd SpiderwomanGame
# Replace Program.cs with this version
dotnet run


The game runs entirely in the terminal/console.

How to Play

When the game starts, Spiderwoman has secretly chosen a letter (A–Z).
You will not see the letter, but the game checks every word you type against it.

Enter a word (must be between 4 and 8 characters long).

If your word does not contain the hidden letter, you are told it’s not in that word.

If your word does contain the letter, you are told “YES – IT’S ONE OF THOSE.”

When the letter is in your word, you are asked if you want to guess.

Type Y (yes) to make a guess.

Type N (no) to continue testing more words.

If you guess correctly, you escape Spiderwoman:

OK – YOU CAN GO (THIS TIME)


If you guess incorrectly, or if you take too many turns, you lose:

YOU ARE TOO LATE
YOU ARE NOW A FLY

Rules

Each word you enter counts as one go.

If you exceed 15 goes without guessing correctly, you automatically lose .

Word length must be 4–8 characters; too short or too long words are rejected .

Only one correct guess lets you escape.

One wrong guess ends the game immediately.

Strategy Tips

Try different words to eliminate unlikely letters.

Use words that cover common vowels and consonants to test quickly.

Don’t waste all your turns—decide carefully when to guess!

Ending

Win condition: Correctly guess the hidden letter.

Lose condition:

Wrong guess, or

Exceed 15 words without a correct guess.

