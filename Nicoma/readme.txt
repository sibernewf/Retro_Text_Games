NICOMA – The Boomerang Puzzle

An ancient number trick from the Arithmetica of Nicomachus (A.D. ~120)

📜 Objective

The computer will guess the number you are thinking of — without you telling it directly.

You think of a number between 1 and 100 (inclusive),
perform a few small divisions, and report only the remainders.
The computer will then instantly deduce your original number.

🗝 Historical Background

The “boomerang” puzzle is one of the oldest known arithmetical tricks.
The version here comes from the ancient mathematician Nicomachus,
who described it almost 1,900 years ago.

🧾 How to Play

Think of any whole number between 1 and 100.
Keep it secret — don’t tell the computer.

The computer will ask:

“Your number divided by 3 has a remainder of?”

“Your number divided by 5 has a remainder of?”

“Your number divided by 7 has a remainder of?”

You enter each remainder (a number between 0 and one less than the divisor).

The computer “thinks” for a moment… and then announces your number.

You confirm with YES or NO:

If YES: The game celebrates and offers to try again.

If NO: The game reports an arithmetic error — meaning your remainders didn’t match any number from 1 to 100.

🔍 How It Works

The trick uses the Chinese Remainder Theorem.

Because 3, 5, and 7 are pairwise coprime, the combination of remainders uniquely identifies a number from 0 to 104.

The program reconstructs the number with:

X = (remainder3 × 70) + (remainder5 × 21) + (remainder7 × 15)


Then it adjusts X into the range 1–100.

💡 Tips

Always double-check your remainder calculations before typing them in.

If you make a mistake, the computer will say it cannot match your “arithmetic”.

You can play multiple rounds without restarting.

📍 Example Session
NICOMA — BOOMERANG PUZZLE FROM A.D. 120

PLEASE THINK OF A NUMBER BETWEEN 1 AND 100.

YOUR NUMBER DIVIDED BY 3 HAS A REMAINDER OF? 2
YOUR NUMBER DIVIDED BY 5 HAS A REMAINDER OF? 0
YOUR NUMBER DIVIDED BY 7 HAS A REMAINDER OF? 5

LET ME THINK A MOMENT...

YOUR NUMBER WAS 65, RIGHT? YES
HOW ABOUT THAT!

LET'S TRY ANOTHER.
