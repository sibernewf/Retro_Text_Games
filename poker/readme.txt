POKER PLAY DRAW POKER
Description
In this game, you play draw poker with the computer as your
opponent. At the start of the game, each person has.$200.
The game ends when either opponent runs out of money (if you
run short, the computer giveS you a chance to sell your
wristwatch or diamond tie tack).
The computer opens the betting before the draw: you open the
betting after the draw. If you don't have a hand that's worth
anything and want to fold', bet O. Prior to the draw, to check
the draw, you may bet .5. Of course, if the computer has
betted, you must match bets (see his bet) in order to draw or,
if your hand looks good either before or after the draw, you
may always raise the bet.
Source
Thanks to A.E. Sapega for submitting this program to DECUS
(BASIC a-SS6). Its author is:
A. Christopher Hall
Trinity College
Hartford, CT 06106


PLAYER DOCUMENTATION

Game: Draw Poker – Player vs Computer
Author of C# version: Adapted from A. Christopher Hall’s original BASIC listing in 101 BASIC Computer Games

🎯 Objective

Your goal is to win all of the computer’s money before it wins all of yours.
You each start with $200 and play classic five-card draw poker:

$5 ante each hand.

The computer opens the betting before the draw.

You open the betting after the draw.

First to run out of money loses.

If you run short on cash, you can sell your wristwatch and diamond tie tack to continue.

📍 Game Flow
1. Starting the Game

The game greets you, explains the ante and betting rules, and asks for your name.

You and the dealer both start with $200.

2. Ante

At the start of each hand, both players pay $5 into the pot automatically (the ante).

If you can’t afford the ante, the game will offer:

Sell your wristwatch for $75

Sell your diamond tie tack for $125

Declining both ends the game.

3. Dealing the Cards

You receive 5 cards face-up (to you).

The dealer’s 5 cards are hidden.

Card numbers are shown next to each card (1–5) for selecting discards later.

4. First Betting Round (Before the Draw)

The dealer opens:

It may check (bet $0) or bet an amount based on its hand strength.

If the dealer checks, you can:

Check (enter 5) to pass without raising.

Bet (enter any amount you can afford) to open the betting.

Fold (enter 0) to give up the pot immediately.

If the dealer bets, you must:

Call by matching the bet.

Raise by betting more (not required in this version before draw).

Fold by entering 0.

5. Drawing Cards

You’ll be asked:
"NOW WE DRAW — HOW MANY CARDS DO YOU WANT?"
Enter 0 to stand pat (keep your hand) or a number 1–3.

Then, if you’re discarding, enter the card numbers (from your hand display) separated by spaces.

The dealer will also draw based on a basic strategy.

6. Second Betting Round (After the Draw)

You open this time:

Enter 0 to check (if no bet is in play) or fold (if a raise happened).

Enter any positive dollar amount up to what you can afford.

The dealer may call or raise you:

If the dealer raises, you must match the raise or fold.

7. Showdown

Both hands are revealed.

The hands are ranked:

Straight Flush

Four of a Kind

Full House

Flush

Straight

Three of a Kind

Two Pair

Pair

High Card

Tie-breaking is done by comparing the highest card(s) in the relevant category.

Winner takes the pot.

8. Continue or Quit

After each hand, the game shows your money and the dealer’s money.

You’ll be asked:

DO YOU WISH TO CONTINUE? (YES/NO)


YES → Play the next hand.

NO → Quit the game.

🖥 Input Guide

At betting prompts:

0 → Fold immediately.

5 → “Check” (only valid before the draw when no one has bet).

Any other whole dollar amount → Place that bet.

At draw prompts:

Number of cards to discard: 0 to keep your hand, 1–3 to draw new cards.

Which cards to discard: space-separated card numbers (from your hand display).

At Yes/No prompts:

YES or Y → Yes

NO or N → No

💰 Selling Valuables

If you run out of cash:

You’ll be offered $75 for your wristwatch (if you still have it).

Then $125 for your diamond tie tack (if you still have it).

Declining both means you’re busted and the game ends.

📝 Example Turn
YOUR HAND:
 1 -- Q♥
 2 -- 7♣
 3 -- 7♠
 4 -- 4♦
 5 -- J♦

I'LL OPEN WITH $12.
WHAT IS YOUR BET? 12

NOW WE DRAW — HOW MANY CARDS DO YOU WANT? 2
WHAT ARE THEIR NUMBERS? 2 4

YOUR HAND:
 1 -- Q♥
 2 -- 9♦
 3 -- 7♠
 4 -- 5♠
 5 -- J♦

WHAT IS YOUR BET? 20
I'LL SEE YOU.
NOW WE COMPARE HANDS...

🃏 Tips for Play

Fold weak hands early to conserve money.

Use the dealer’s check as a sign it might have a weaker hand.

Keep track of how many times you’ve sold valuables — after both are gone, you’re playing for survival.

Don’t be afraid to check after the draw if you’re unsure.
