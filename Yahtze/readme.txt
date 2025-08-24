🎲 Player Documentation – YAHTZE (Yahtzee)
📝 The Goal

Score the highest total across 13 rounds by rolling 5 dice and assigning results to scoring categories.

Each player has exactly one chance per category. Once used, it cannot be used again.

👥 Players

Supports 1–15 players.

Each round, every player takes one turn.

The game lasts exactly 13 rounds (all categories used).

🎮 How to Play

Rolling the Dice

On your turn, the computer rolls 5 dice for you.

You may roll up to 3 times (initial roll + two re-rolls).

After the first or second roll, you may choose to re-roll some of the dice:

The program asks:
HOW MANY DO YOU WANT TO CHANGE?

Enter 0–5.

If >0, it will then ask WHICH?. Enter dice positions (1–5).

Those dice are re-rolled; the others stay.

Choosing a Category

After rolling, you must assign your dice to a scoring category:

Type the category name (e.g., FIVES, YAHTZEE, CHANCE).

Type SUMMARY to see which categories you’ve already used and your current totals.

Type ZERO to scratch a category (score 0 in it).

Scoring Rules

Aces – Count and add all 1’s.

Twos – Count and add all 2’s.

Threes – Count and add all 3’s.

Fours – Count and add all 4’s.

Fives – Count and add all 5’s.

Sixes – Count and add all 6’s.

Three of a Kind – If 3 dice the same, score the total of all 5 dice. Otherwise 0.

Four of a Kind – If 4 dice the same, score the total of all 5 dice. Otherwise 0.

Full House – 3 of one kind and 2 of another = 25 points.

Small Straight – Sequence of 4 in a row (e.g., 2-3-4-5) = 30 points.

Large Straight – Sequence of 5 in a row (e.g., 1-2-3-4-5) = 40 points.

Yahtzee – All 5 dice the same = 50 points.

Chance – Score the total of all 5 dice.

Zero – If stuck, eliminate a category (score 0).

Bonuses

If the total from Aces through Sixes ≥ 63 points, you earn a +35 bonus.

🏆 Winning

After 13 rounds, totals are displayed.

The player with the highest grand total wins.

📋 Example Turn
DAVE'S TURN
YOU HAVE 5 4 3 2 2
THIS IS YOUR 2ND OF 3 ROLLS
HOW MANY DO YOU WANT TO CHANGE ? 1
WHICH ? 5

YOU HAVE 5 4 3 2 1
THIS IS YOUR LAST ROLL
HOW MANY DO YOU WANT TO CHANGE ? 0
HOW DO YOU WANT THIS ROUND SCORED ? LG. STRAIGHT

DAVE YOU GET A SCORE OF 40 FOR THIS ROUND

💡 Tips

Save YAHTZEE (50 points) for when you roll 5 of a kind.

Use CHANCE to dump bad rolls for some points.

Use ZERO wisely if you can’t fit a roll into any category.

Always keep track of your upper section subtotal—try to reach 63 to claim the 35-point bonus.

▶️ To Play

Double-click run.bat in the game folder.
Follow prompts in the console.
