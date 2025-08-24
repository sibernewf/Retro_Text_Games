Horses:
=============================
Enter bets like:
 - BET NO. 2 1 100
 - BET NO. 5 3 20
 - BET NO. 0
 - Format: horse, position, amount
   - horse: 1..8
   - position: 1=Win, 2=Place, 3=Show
   - amount: $2–$300
 - Watch the race unfold and see your mutuel returns.

Player Documentation — HORSES (Betting at a Horserace)
🎯 Objective
Place bets on a simulated one-mile race featuring 8 thoroughbreds.
You can bet Win, Place, or Show on any horse (or multiple horses).
After the race, you’ll see the mutuel payouts and how each of your tickets did.

🐎 The Field
Eight named horses run every race. Their form/strength is randomized each race, so favorites and longshots vary.

💵 Bet Types
Win (1): Your horse must finish 1st.

Place (2): Your horse must finish 1st or 2nd.

Show (3): Your horse must finish 1st, 2nd, or 3rd.

Minimum wager: $2  Maximum wager: $300
You may enter any number of bets before the race.

⌨️ How to Enter Bets
At the prompt BET NO., type:

arduino
Copy
Edit
horse, position, amount
horse = 1..8

position = 1 (Win), 2 (Place), 3 (Show)

amount = 2–300 (dollars)

Examples

pgsql
Copy
Edit
2,1,50      # $50 to WIN on horse 2
5,3,10      # $10 to SHOW on horse 5
7 2 20      # (spaces also OK): $20 to PLACE on horse 7
0           # finish betting and start the race
Enter 0 (or a blank line) to end betting and run the race.

🏁 The Race Display
The program prints the running order at several points:

They’re off and running (break from the gate)

1/4 mile, Halfway, Rounding the turn, Stretch

Finish

Each snapshot shows position, horse name, and lengths behind the leader.

💰 Payouts & Tickets
After the finish you’ll see Mutuels Paid for the podium horses and a line for each of your bets:

Winning tickets show: YOU COLLECT 498.50 ON CITATION

Losing tickets show: TEAR UP YOUR TICKET ON WHIRLAWAY

A summary line totals your winnings or losses

Payouts are quoted per $2 base ticket. Your actual return scales with your wager (e.g., a $10 bet = 5 × $2 ticket).

▶ How to Play
Run the game (run.bat or dotnet run).

Enter as many bets as you like using the format above.

Enter 0 to start the race.

Watch the race and review your results.

Choose whether to run another race.

💡 Tips
 - Mix bet types: Win for bigger payouts, Show for safer returns.
 - Don’t spread too thin—two or three focused tickets often beat many tiny ones.
 - Longshots occasionally win; consider a small Show bet on an outsider.

🔁 Start Another Race
At the end you’ll be asked:

  RUN ANOTHER RACE (YES OR NO)?

Type YES to bet again or NO to quit.
