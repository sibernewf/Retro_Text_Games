📘 Player Documentation – WORD
🎮 The Goal

Guess the mystery 5-letter word.
On each turn you guess an entire 5-letter string (a real word or just 5 letters like ABCDE). The computer tells you:

Common letters (letters your guess shares with the secret, in any position).

Exact matches (which positions are correct), shown as a pattern of letters and dashes (e.g., C--MP).

Keep guessing until you find the secret word—or type ? to give up and reveal it.

🕹️ How to Play

When prompted, type a 5-letter guess (letters only).

The game prints two clues:

THERE WERE N MATCHES AND THE COMMON LETTERS WERE... CLM

N = number of common letters.

The list (CLM) are the shared letters (order not meaningful).

FROM THE EXACT LETTER MATCHES, YOU KNOW........ C--MP

A letter = correct letter in the correct position.

A dash = wrong letter at that position.

Repeat with a new 5-letter guess using what you’ve learned.

Type ? to reveal the secret word and end the round.

After solving (or giving up), you can choose to play again.

Note: Only 5-letter guesses are accepted. The word list can be edited in the program’s data section.

📋 Example Round
YOU ARE STARTING A NEW GAME...

GUESS A FIVE-LETTER WORD?  ABCDE
THERE WERE 1 MATCHES AND THE COMMON LETTERS WERE...  C
FROM THE EXACT LETTER MATCHES, YOU KNOW............  -----

GUESS A FIVE-LETTER WORD?  COLUMN
YOU MUST GUESS A 5-LETTER WORD.  START AGAIN

GUESS A FIVE-LETTER WORD?  CLUMP
THERE WERE 4 MATCHES AND THE COMMON LETTERS WERE...  CLMP
FROM THE EXACT LETTER MATCHES, YOU KNOW............  C--MP

GUESS A FIVE-LETTER WORD?  CLUMP
YOU HAVE GUESSED THE WORD.  IT TOOK 6 GUESSES!

💡 Tips

Start with a guess that uses five different common letters (e.g., ARISE, TONED) to gather information quickly.

Focus on the exact-match pattern to lock down positions.

Use common-letter list to avoid repeating letters the secret doesn’t contain.

If stuck, try a probe word that tests new letters in the positions still showing dashes.

▶️ To Play

Double-click run.bat in the game’s folder.

This builds and runs the program and keeps the window open when it finishes.